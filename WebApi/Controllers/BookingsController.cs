using Aufguss.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using WebApi.Models;
using WebApi.Services;
using WebApi.Services.Email;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly NotificationService _notificationService;

        private readonly ResendEmailService _resendEmailService;

        private readonly IBackgroundEmailQueue _emailQueue;

        private readonly IBookingManager _bookingManager;

        public BookingsController(ApplicationDbContext context, IBookingManager bookingManager)
        {
            _context = context;
            _bookingManager = bookingManager;

        }
        [AllowAnonymous]
        [HttpGet("public-stats")]
        public async Task<IActionResult> GetPublicBookingStats()
        {
            var stats = await _context.Events
                .Select(e => new
                {
                    e.Id,
                    e.MaxSlots,
                    BookedCount = _context.Bookings.Count(b => b.EventId == e.Id && !b.Unbooked && !b.Reserve)
                })
                .ToListAsync();

            return Ok(stats);
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetBookings()
        {
            var bookings = await _context.Bookings.OrderBy(b => b.Id).ToListAsync();
            return Ok(bookings);
        }
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyBookings()
        {
            // Extract user identifier from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User identifier not found.");

            var myBookings = await _context.Bookings
                .Include(b => b.Event)
                .Where(b => b.UserId == userId && !b.Unbooked)
                .OrderByDescending(b => b.Id)
                .ToListAsync();

            return Ok(myBookings);
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            return Ok(booking);
        }
        [AllowAnonymous]
        [HttpGet("cancel-booking")]
        public async Task<IActionResult> CancelBooking([FromQuery] string token)
        {
            var result = await _bookingManager.CancelBookingByTokenAsync(token);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        //[HttpGet("cancel-booking")]
        //public async Task<IActionResult> CancelBooking([FromQuery] string token)
        //{
        //    if (string.IsNullOrWhiteSpace(token))
        //        return BadRequest("Token saknas");

        //    var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.CancellationToken == token);

        //    if (booking == null || booking.Unbooked)
        //        return NotFound("Ogiltig eller redan avbokad bokning");

        //    booking.Unbooked = true;
        //    booking.UnbookedAt = DateTime.UtcNow;

        //    await _context.SaveChangesAsync();

        //    return Ok("Bokningen har avbokats.");
        //}
        [Authorize]
        [HttpPost("{id}/unbook")]
        public async Task<IActionResult> UnbookBooking(int id)
        {
            // Get the identity of the actor (who performed this)
            var performedBy = User?.Identity?.Name ?? "Anonymous";

            // Optional: detect if admin
            var isAdmin = User?.IsInRole("Admin") ?? false;

            var result = await _bookingManager.UnbookAsync(id, performedBy, isAdmin);

            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Message);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateBooking(BookingDto dto)
        {
            var result = await _bookingManager.CreateBookingAsync(dto);

            if (!result.Success)
            {
                Console.WriteLine($"⚠️ Booking failed: {result.Message}");
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditBooking(int id, BookingDto dto)
        {
            var result = await _bookingManager.EditBookingAsync(id, dto);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Booking);
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var result = await _bookingManager.DeleteBookingAsync(id);
            if (!result.Success) return NotFound(result.Message);
            return Ok(result.Message);
        }

        [Authorize]
        [HttpPost("{bookingId}/confirm-attendance")]
        public async Task<IActionResult> ConfirmAttendance(int bookingId, [FromBody] ConfirmAttendanceDto dto)
        {
            // 1️⃣ Find the booking
            var booking = await _context.Bookings
                .Include(b => b.Event)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return NotFound($"Bokning med ID {bookingId} hittades inte.");

            var evt = booking.Event;
            if (evt == null)
                return NotFound($"Event för bokningen hittades inte.");

            // 2️⃣ Time constraint — within 2 hours of start
            var now = DateTime.Now;
            var timeUntilStart = evt.Start - now;
            if (timeUntilStart > TimeSpan.FromHours(2))
                return BadRequest("För tidigt att bekräfta närvaro (mer än 2 timmar kvar till start).");

            // 3️⃣ Geolocation constraint — within 500 m of the venue
            double eventLat = 59.04406605697291;   // Freja Bad & Gym
            double eventLng = 17.3156782887916;

            var distanceMeters = CalculateDistanceMeters(dto.UserLat, dto.UserLng, eventLat, eventLng);
            if (distanceMeters > 500)
                return BadRequest($"Du är för långt bort ({Math.Round(distanceMeters)} m). Max 500 m tillåtet.");

            // 4️⃣ Mark attendance as confirmed
            booking.AttendanceConfirmed = true;
            booking.AttendanceConfirmedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ {booking.Name} bekräftade närvaro för event '{evt.Title}' ({Math.Round(distanceMeters)} m bort).");

            return Ok(new
            {
                message = "Närvaro bekräftad",
                distance = Math.Round(distanceMeters)
            });
        }
        private static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // meters
            var latRad1 = Math.PI * lat1 / 180;
            var latRad2 = Math.PI * lat2 / 180;
            var deltaLat = Math.PI * (lat2 - lat1) / 180;
            var deltaLon = Math.PI * (lon2 - lon1) / 180;

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(latRad1) * Math.Cos(latRad2) *
                    Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }


    }
}
