using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Services;
using WebApi.Services.Email;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        private readonly NotificationService _notificationService;

        private readonly ResendEmailService _resendEmailService;

        private readonly IBackgroundEmailQueue _emailQueue;

        public BookingsController(ApplicationDbContext context, ResendEmailService resendEmailService)
        {
            this.context = context;
            _resendEmailService = resendEmailService;

        }

        [HttpGet]
        public IActionResult GetBookings()
        {
            var bookings = context.Bookings.OrderBy(b => b.Id).ToList();
            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public IActionResult GetBooking(int id)
        {
            var booking = context.Bookings.Find(id);
            if (booking == null)
            {
                return NotFound();
            }

            return Ok(booking);
        }

        [HttpGet("cancel-booking")]
        public async Task<IActionResult> CancelBooking([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Token saknas");

            var booking = await context.Bookings.FirstOrDefaultAsync(b => b.CancellationToken == token);

            if (booking == null || booking.Unbooked)
                return NotFound("Ogiltig eller redan avbokad bokning");

            booking.Unbooked = true;
            booking.UnbookedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return Ok("Bokningen har avbokats.");
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking(BookingDto bookingDto)
        {
            var settings = context.SiteSettings.FirstOrDefault();
            Console.WriteLine("CreateBooking called.");

            // 1. Check if user already booked
            var alreadyBooked = context.Bookings.Any(b =>
                                b.EventId == bookingDto.EventId &&
                                b.Email == bookingDto.Email &&
                                !b.Unbooked &&
                                !b.Reserve);

            if (alreadyBooked)
                return BadRequest(new { message = "Du är redan bokad till det här passet" });

            // 2. Check total active bookings
            var countActiveBookings = (from b in context.Bookings
                                       join e in context.Events on b.EventId equals e.Id
                                       where b.Email == bookingDto.Email &&
                                             !b.Unbooked &&
                                             !b.Reserve &&
                                             e.Start > DateTime.Now
                                       select b).Count();

            if (countActiveBookings >= settings.MaxBookingsAufguss)
                return BadRequest(new { message = $"För många aktiva bokningar, max {settings.MaxBookingsAufguss}" });

            // 3. Create new booking
            var currEvent = context.Events.FirstOrDefault(e => e.Id == bookingDto.EventId);
            if (currEvent == null)
                return NotFound(new { message = "Eventet hittades inte." });

            var existingBookings = context.Bookings.Count(b =>
                b.EventId == bookingDto.EventId &&
                !b.Unbooked &&
                !b.Reserve);

            bool isReserve = existingBookings >= currEvent.MaxSlots;

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

            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            Console.WriteLine("Booking saved. Sending email...");

            // 4. Prepare email content
            var cancelUrl = $"https://aufgussapi-d6f4dae9g9h3a2h8.northeurope-01.azurewebsites.net/api/bookings/cancel-booking?token={booking.CancellationToken}";
            string statusMessage = isReserve ? "Du är placerad på reservlistan till" : "Du är inbokad till";

            var htmlMessage = $@"
<div style='font-family: Arial, sans-serif; font-size: 16px; color: #333;'>
    <p><strong>Tack för din bokning {booking.Name}!</strong></p>
    <p>{statusMessage} <strong>{currEvent.Title}</strong> den {currEvent.Start:yyyy-MM-dd HH:mm}.</p>
    <p>Om du vill avboka din plats, klicka på länken nedan:</p>
    <p><a href='{cancelUrl}' style='color: #007bff; text-decoration: none;'>Avboka din plats</a></p>
    <br>
    <p>Vi ses på <strong>Freja Bad & Gym</strong>!</p>
</div>";

            try
            {
                // Call the ResendEmailService directly
                await _resendEmailService.SendEmailAsync(
                    booking.Email,
                    "Bokningsbekräftelse Aufguss",
                    htmlMessage
                );
                Console.WriteLine($"Email sent to: {booking.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send email: " + ex.Message);
                // Don't fail the booking if email sending fails
            }

            return Ok(new { message = $"{statusMessage} till det här passet" });
        }


        [HttpPut("Unbook/{id}")]
        public IActionResult Unbook(int id)
        {
            var booking = context.Bookings.Find(id);
            if (booking == null)
            {
                return NotFound();
            }

            // Unbooking
            booking.Unbooked = true;
            booking.UnbookedAt = DateTime.Now;

            context.SaveChanges();

            CheckReserveList(booking);

            //var bookingsCount = context.Bookings.Where(b => b.EventId == booking.EventId && !b.Reserve && !b.Unbooked).Count();
            //var evt = context.Events.Where(e => e.Id == booking.EventId).FirstOrDefault();

            //if(evt.MaxSlots >= bookingsCount)
            //{
            //    var reserveList = context.Bookings
            //        .Where(b => b.EventId == booking.EventId && b.Reserve)
            //        .OrderBy(b => b.ReserveAt)
            //        .ToList();

            //    if (reserveList.Any())
            //    {
            //        // Need to add notification but also check that the user isn't already actively booked to three events
            //        var promote = reserveList[0];
            //        promote.Reserve = false;
            //        promote.Unbooked = false;
            //        promote.BookedAt = DateTime.Now;
            //    }
            //}


            return Ok(booking);
        }


        [HttpPut("{id}")]
        public IActionResult EditBooking(int id, BookingDto bookingDto)
        {
            var booking = context.Bookings.Find(id);
            if (booking == null)
            {
                return NotFound();
            }

            if (bookingDto.Unbooked == true)
            {
                // Unbooking
                booking.Unbooked = true;
                booking.UnbookedAt = DateTime.Now;

                var reserveList = context.Bookings
                    .Where(b => b.Reserve == true && b.EventId == booking.EventId)
                    .OrderBy(b => b.ReserveAt)
                    .ToList();

                if (reserveList.Any())
                {
                    // Need to add notification but also check that the user isn't already actively booked to three events
                    var promote = reserveList[0];
                    promote.Reserve = false;
                    promote.Unbooked = false;
                    promote.BookedAt = DateTime.Now;
                }
            }
            else
            {
                // Re-booking (from Unbooked == true ➝ false), must check capacity
                if (booking.Unbooked == true && bookingDto.Unbooked == false)
                {
                    var evt = context.Events.Find(bookingDto.EventId);
                    if (evt == null)
                    {
                        return BadRequest("Event not found.");
                    }

                    // Count current bookings (not unbooked, not reserve)
                    var currentBookings = context.Bookings
                        .Where(b => b.EventId == bookingDto.EventId && b.Unbooked == false && b.Reserve == false)
                        .Count();

                    if (currentBookings >= evt.MaxSlots)
                    {
                        return Conflict("No spots available.");
                    }

                    booking.Unbooked = false;
                    booking.UnbookedAt = null; // Optional: clear if you want
                }

                // Update other fields
                booking.Name = bookingDto.Name;
                booking.Email = bookingDto.Email;
                booking.Tel = bookingDto.Tel;
                booking.BookedAt = bookingDto.BookedAt;
                booking.EventId = bookingDto.EventId;
            }

            context.SaveChanges();

            return Ok(booking);
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteBooking(int id)
        {
            var booking = context.Bookings.Find(id);
            if (booking == null)
            {
                return NotFound();
            }

            context.Bookings.Remove(booking);
            context.SaveChanges();

            CheckReserveList(booking);

            return Ok();
        }

        public async Task CheckReserveList(Booking booking)
        {
            var bookingsCount = context.Bookings.Where(b => b.EventId == booking.EventId && !b.Reserve && !b.Unbooked).Count();
            var evt = context.Events.Where(e => e.Id == booking.EventId).FirstOrDefault();

            if (evt.MaxSlots > bookingsCount)
            {

                var reserveList = context.Bookings.Where(b => b.EventId == booking.EventId && b.Reserve == true).OrderBy(b => b.ReserveAt).ToList();

                if (reserveList.Any())
                {
                    reserveList[0].Reserve = false;
                    reserveList[0].BookedAt = DateTime.Now;
                }
            }
            context.SaveChanges();
        }

    }

}
