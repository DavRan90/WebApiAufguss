using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Services.Email;

namespace WebApi.Services
{
    public class BookingManager : IBookingManager
    {
        private readonly ApplicationDbContext _context;
        private readonly ResendEmailService _emailService;

        public BookingManager(ApplicationDbContext context, ResendEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<(bool Success, string Message)> CreateBookingAsync(BookingDto bookingDto)
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            var currEvent = await _context.Events.FindAsync(bookingDto.EventId);

            if (currEvent == null)
                return (false, "Eventet hittades inte.");

            // Check if user already booked
            var alreadyBooked = await _context.Bookings.AnyAsync(b =>
                b.EventId == bookingDto.EventId &&
                b.Email == bookingDto.Email &&
                !b.Unbooked &&
                !b.Reserve);

            if (alreadyBooked)
                return (false, "Du är redan bokad till det här passet");

            // Check total active bookings
            var countActive = await (from b in _context.Bookings
                                     join e in _context.Events on b.EventId equals e.Id
                                     where b.Email == bookingDto.Email &&
                                           !b.Unbooked &&
                                           !b.Reserve &&
                                           e.Start > DateTime.Now
                                     select b).CountAsync();

            if (countActive >= settings.MaxBookingsAufguss)
                return (false, $"För många aktiva bokningar, max {settings.MaxBookingsAufguss}");

            // Determine if reserve
            var existingCount = await _context.Bookings
                .CountAsync(b => b.EventId == bookingDto.EventId && !b.Unbooked && !b.Reserve);

            bool isReserve = existingCount >= currEvent.MaxSlots;

            var booking = new Booking
            {
                Name = bookingDto.Name,
                Email = bookingDto.Email,
                Tel = bookingDto.Tel,
                UserId = bookingDto.UserId,
                BookedAt = DateTime.Now,
                EventId = bookingDto.EventId,
                CancellationToken = Guid.NewGuid().ToString("N"),
                Reserve = isReserve,
                ReserveAt = isReserve ? DateTime.Now : null
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            await LogBookingActionAsync(booking.Id, "Booked", booking.Email,
                isReserve ? "Placed on reserve list" : "Confirmed booking");

            // Send email confirmation
            var cancelUrl = $"https://yourdomain.com/api/bookings/cancel-booking?token={booking.CancellationToken}";
            var status = isReserve ? "Du är placerad på reservlistan till" : "Du är inbokad till";

            var html = $@"
            <div style='font-family: Arial, sans-serif; font-size: 16px; color: #333;'>
                <p><strong>Tack för din bokning {booking.Name}!</strong></p>
                <p>{status} <strong>{currEvent.Title}</strong> den {currEvent.Start:yyyy-MM-dd HH:mm}.</p>
                <p><a href='{cancelUrl}'>Avboka din plats</a></p>
            </div>";

            try
            {
                await _emailService.SendEmailAsync(booking.Email, "Bokningsbekräftelse Aufguss", html);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Email failed: " + ex.Message);
            }

            return (true, $"{status} det här passet");
        }

        public async Task<(bool Success, string Message, Booking? Booking)> EditBookingAsync(int id, BookingDto bookingDto)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return (false, "Booking not found", null);

            if (bookingDto.Unbooked)
            {
                // Unbooking
                booking.Unbooked = true;
                booking.UnbookedAt = DateTime.UtcNow;

                // Promote from reserve if needed
                await CheckReserveListAsync(booking);
            }
            else
            {
                // Re-booking
                if (booking.Unbooked && !bookingDto.Unbooked)
                {
                    var evt = await _context.Events.FindAsync(bookingDto.EventId);
                    if (evt == null)
                        return (false, "Event not found", null);

                    var currentBookings = await _context.Bookings
                        .Where(b => b.EventId == bookingDto.EventId && !b.Unbooked && !b.Reserve)
                        .CountAsync();

                    if (currentBookings >= evt.MaxSlots)
                        return (false, "No spots available", null);

                    booking.Unbooked = false;
                    booking.UnbookedAt = null;
                }

                // Update other fields
                booking.Name = bookingDto.Name;
                booking.Email = bookingDto.Email;
                booking.Tel = bookingDto.Tel;
                booking.BookedAt = bookingDto.BookedAt;
                booking.EventId = bookingDto.EventId;
            }

            await _context.SaveChangesAsync();
            return (true, "Booking updated", booking);
        }


        public async Task<(bool Success, string Message)> UnbookAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return (false, "Bokningen hittades inte.");

            booking.Unbooked = true;
            booking.UnbookedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            await CheckReserveListAsync(booking);
            await LogBookingActionAsync(booking.Id, "Unbooked", booking.Email);

            return (true, "Bokningen har avbokats.");
        }

        public async Task<(bool Success, string Message)> DeleteBookingAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return (false, "Bokningen hittades inte.");

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            await CheckReserveListAsync(booking);
            await LogBookingActionAsync(booking.Id, "Deleted", booking.Email);

            return (true, "Bokningen har tagits bort.");
        }

        public async Task CheckReserveListAsync(Booking booking)
        {
            var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == booking.EventId);
            if (evt == null) return;

            // Use a transaction to prevent race conditions
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Count current active bookings
                var bookingsCount = await _context.Bookings
                    .Where(b => b.EventId == booking.EventId && !b.Reserve && !b.Unbooked)
                    .CountAsync();

                if (evt.MaxSlots > bookingsCount)
                {
                    // Find the first person on the reserve list
                    var reserveList = await _context.Bookings
                        .Where(b => b.EventId == booking.EventId && b.Reserve && !b.Unbooked)
                        .OrderBy(b => b.ReserveAt)
                        .ToListAsync();

                    if (reserveList.Any())
                    {
                        var promoted = reserveList.First();
                        promoted.Reserve = false;
                        promoted.BookedAt = DateTime.Now;

                        await _context.SaveChangesAsync();

                        // Commit transaction so no one else can promote at the same time
                        await transaction.CommitAsync();

                        // Send promotion email outside the transaction (to avoid holding DB lock)
                        try
                        {
                            var cancelUrl = $"https://yourdomain.com/api/bookings/cancel-booking?token={promoted.CancellationToken}";
                            var html = $@"
                            <div style='font-family: Arial, sans-serif; font-size: 16px; color: #333;'>
                                <p><strong>Grattis {promoted.Name}!</strong></p>
                                <p>En plats har blivit ledig och du har nu fått en ordinarie plats till <strong>{evt.Title}</strong> den {evt.Start:yyyy-MM-dd HH:mm}.</p>
                                <p><a href='{cancelUrl}'>Avboka din plats</a></p>
                            </div>";

                            await _emailService.SendEmailAsync(promoted.Email, "Du har fått en plats!", html);
                            await LogBookingActionAsync(promoted.Id, "Promoted", promoted.Email, $"Promoted from reserve list for event {evt.Title}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to send promotion email: " + ex.Message);
                        }
                    }
                }
                else
                {
                    await transaction.RollbackAsync();
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        private async Task LogBookingActionAsync(int bookingId, string action, string userEmail, string? details = null)
        {
            var audit = new BookingAudit
            {
                BookingId = bookingId,
                Action = action,
                UserEmail = userEmail,
                Timestamp = DateTime.UtcNow,
                Details = details
            };

            _context.BookingAudits.Add(audit);
            await _context.SaveChangesAsync();
        }

    }
}
