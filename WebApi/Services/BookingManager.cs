using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Services.Email;

namespace WebApi.Services
{
    public class BookingManager : IBookingManager
    {
        private readonly ApplicationDbContext _context;
        private readonly ResendEmailService _emailService;
        private readonly IAuditService _audit;

        public BookingManager(ApplicationDbContext context, ResendEmailService emailService, IAuditService audit)
        {
            _context = context;
            _emailService = emailService;
            _audit = audit;
        }

        /// <summary>
        /// Creates a new booking or adds the user to the reserve list if the event is full.
        /// </summary>
        public async Task<(bool Success, string Message)> CreateBookingAsync(BookingDto bookingDto)
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            var currEvent = await _context.Events.FindAsync(bookingDto.EventId);

            if (currEvent == null)
                return (false, "Eventet hittades inte.");

            // Check if already booked
            bool alreadyBooked = await _context.Bookings.AnyAsync(b =>
                b.EventId == bookingDto.EventId &&
                b.Email == bookingDto.Email &&
                !b.Unbooked &&
                !b.Reserve);

            if (alreadyBooked)
                return (false, "Du är redan bokad till det här passet.");

            // Check max active bookings per user
            int activeBookings = await (from b in _context.Bookings
                                        join e in _context.Events on b.EventId equals e.Id
                                        where b.Email == bookingDto.Email &&
                                              !b.Unbooked &&
                                              !b.Reserve &&
                                              e.Start > DateTime.Now
                                        select b).CountAsync();

            if (activeBookings >= settings.MaxBookingsAufguss)
                return (false, $"För många aktiva bokningar, max {settings.MaxBookingsAufguss} tillåtna.");

            // Determine if reserve
            int currentCount = await _context.Bookings
                .CountAsync(b => b.EventId == bookingDto.EventId && !b.Unbooked && !b.Reserve);

            bool isReserve = currentCount >= currEvent.MaxSlots;

            // Create booking entity
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

            // Log audit
            var logAction = isReserve ? "Reservlista" : "Inbokad";
            var logDetails = isReserve
                ? $"Användaren {booking.Email} placerades på reservlistan till eventet '{currEvent.Title}'."
                : $"Användaren {booking.Email} bokade en plats till eventet '{currEvent.Title}'.";

            await _audit.LogBookingAsync(booking.Id, logAction, booking.Email, logDetails);

            // Send confirmation email
            var cancelUrl = $"https://yourdomain.com/api/bookings/cancel-booking?token={booking.CancellationToken}";
            var statusText = isReserve ? "Du är placerad på reservlistan till" : "Du är inbokad till";

            var html = $@"
                <div style='font-family: Arial, sans-serif; font-size: 16px; color: #333;'>
                    <p><strong>Tack för din bokning {booking.Name}!</strong></p>
                    <p>{statusText} <strong>{currEvent.Title}</strong> den {currEvent.Start:yyyy-MM-dd HH:mm}.</p>
                    <p><a href='{cancelUrl}'>Avboka din plats</a></p>
                </div>";

            try
            {
                await _emailService.SendEmailAsync(booking.Email, "Bokningsbekräftelse Aufguss", html);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email failed to {booking.Email}: {ex.Message}");
            }

            return (true, $"{statusText} det här passet.");
        }

        /// <summary>
        /// Edits a booking (used for admin or user-driven updates).
        /// </summary>
        public async Task<(bool Success, string Message, Booking? Booking)> EditBookingAsync(int id, BookingDto bookingDto)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return (false, "Bokningen hittades inte.", null);

            if (bookingDto.Unbooked)
            {
                // Unbook
                booking.Unbooked = true;
                booking.UnbookedAt = DateTime.UtcNow;

                var evt = await _context.Events.FindAsync(booking.EventId);
                await _audit.LogBookingAsync(booking.Id, "Avbokad", booking.Email,
                    $"Användaren {booking.Email} avbokade sin plats till eventet '{evt?.Title}'.");

                await CheckReserveListAsync(booking);
            }
            else
            {
                // Rebook (if previously unbooked)
                if (booking.Unbooked)
                {
                    var evt = await _context.Events.FindAsync(bookingDto.EventId);
                    if (evt == null)
                        return (false, "Eventet hittades inte.", null);

                    int activeCount = await _context.Bookings
                        .CountAsync(b => b.EventId == bookingDto.EventId && !b.Unbooked && !b.Reserve);

                    if (activeCount >= evt.MaxSlots)
                        return (false, "Inga lediga platser finns kvar.", null);

                    booking.Unbooked = false;
                    booking.UnbookedAt = null;

                    await _audit.LogBookingAsync(booking.Id, "Återinbokad", booking.Email,
                        $"Användaren {booking.Email} återinbokades till eventet '{evt.Title}'.");
                }

                booking.Name = bookingDto.Name;
                booking.Email = bookingDto.Email;
                booking.Tel = bookingDto.Tel;
                booking.BookedAt = bookingDto.BookedAt;
                booking.EventId = bookingDto.EventId;
            }

            await _context.SaveChangesAsync();
            return (true, "Bokningen har uppdaterats.", booking);
        }

        /// <summary>
        /// Cancels a booking (used when user cancels via link or UI).
        /// </summary>
        public async Task<(bool Success, string Message)> UnbookAsync(int id)
        {
            var booking = await _context.Bookings.Include(b => b.Event).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null)
                return (false, "Bokningen hittades inte.");

            booking.Unbooked = true;
            booking.UnbookedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            await CheckReserveListAsync(booking);

            await _audit.LogBookingAsync(booking.Id, "Avbokad", booking.Email,
                $"Användaren {booking.Email} avbokade sin plats till eventet '{booking.Event?.Title}'.");

            return (true, "Bokningen har avbokats.");
        }

        /// <summary>
        /// Deletes a booking (used by admin).
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteBookingAsync(int id)
        {
            var booking = await _context.Bookings.Include(b => b.Event).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null)
                return (false, "Bokningen hittades inte.");

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            await CheckReserveListAsync(booking);

            await _audit.LogBookingAsync(booking.Id, "Borttagen", booking.Email,
                $"Bokningen för {booking.Email} togs bort från eventet '{booking.Event?.Title}'.");

            return (true, "Bokningen har tagits bort.");
        }

        /// <summary>
        /// Promotes a user from the reserve list if a slot becomes available.
        /// </summary>
        public async Task CheckReserveListAsync(Booking booking)
        {
            var evt = await _context.Events.FirstOrDefaultAsync(e => e.Id == booking.EventId);
            if (evt == null) return;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                int activeCount = await _context.Bookings
                    .CountAsync(b => b.EventId == booking.EventId && !b.Reserve && !b.Unbooked);

                if (evt.MaxSlots > activeCount)
                {
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
                        await transaction.CommitAsync();

                        // Send promotion email (outside transaction)
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
                            await _audit.LogBookingAsync(promoted.Id, "Inbokad", promoted.Email,
                                $"Deltagaren {promoted.Email} flyttades från reservlistan till ordinarie plats för eventet '{evt.Title}'.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Misslyckades med att skicka e-post till {promoted.Email}: {ex.Message}");
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
    }
}
