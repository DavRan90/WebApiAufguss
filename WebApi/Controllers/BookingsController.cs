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

        private readonly IBookingManager _bookingManager;

        public BookingsController(ApplicationDbContext context, IBookingManager bookingManager)
        {
            this.context = context;
            _bookingManager = bookingManager;

        }

        [HttpGet]
        public async Task<IActionResult> GetBookings()
        {
            var bookings = await context.Bookings.OrderBy(b => b.Id).ToListAsync();
            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            var booking = await context.Bookings.FindAsync(id);
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
        public async Task<IActionResult> CreateBooking(BookingDto dto)
        {
            var result = await _bookingManager.CreateBookingAsync(dto);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditBooking(int id, BookingDto dto)
        {
            var result = await _bookingManager.EditBookingAsync(id, dto);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Booking);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var result = await _bookingManager.DeleteBookingAsync(id);
            if (!result.Success) return NotFound(result.Message);
            return Ok(result.Message);
        }


    }

}
