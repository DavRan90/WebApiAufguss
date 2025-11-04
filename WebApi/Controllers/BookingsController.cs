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

        [HttpGet("cancel-booking")]
        public async Task<IActionResult> CancelBooking([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest("Token saknas");

            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.CancellationToken == token);

            if (booking == null || booking.Unbooked)
                return NotFound("Ogiltig eller redan avbokad bokning");

            booking.Unbooked = true;
            booking.UnbookedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok("Bokningen har avbokats.");
        }
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
        [Authorize]
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
    }
}
