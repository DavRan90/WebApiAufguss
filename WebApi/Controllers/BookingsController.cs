using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        private readonly NotificationService _notificationService;

        private readonly ResendEmailService _resendEmail;

        public BookingsController(ApplicationDbContext context, ResendEmailService resendEmail)
        {
            this.context = context;
            _resendEmail = resendEmail;
        }
        //public BookingsController(ApplicationDbContext context)
        //{
        //    this.context = context;
        //}

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

        //[HttpPost]
        //public IActionResult CreateBooking(BookingDto bookingDto)
        //{

        //    var booking = new Booking
        //    {
        //        Name = bookingDto.Name,
        //        Email = bookingDto.Email,
        //        Tel = bookingDto.Tel,
        //        BookedAt = DateTime.Now,
        //        EventId = bookingDto.EventId,
        //    };

        //    if(bookingDto.Reserve == true)
        //    {
        //        booking.Reserve = true;
        //        booking.ReserveAt = DateTime.Now;
        //    }

        //    context.Bookings.Add(booking);
        //    context.SaveChanges();

        //    return Ok(booking);
        //}

        [HttpPost]
        public async Task<IActionResult> CreateBooking(BookingDto bookingDto)
        {
            Console.WriteLine("CreateBooking called.");

            var alreadyBooked = context.Bookings.Any(b =>
                                b.EventId == bookingDto.EventId &&
                                b.Email == bookingDto.Email &&
                                !b.Unbooked &&
                                !b.Reserve);

            if (alreadyBooked)
            {
                return BadRequest(new { message = "Du är redan bokad till det här passet" });
            }


            var countActiveBookings = (from b in context.Bookings
                                       join e in context.Events on b.EventId equals e.Id
                                       where b.Email == bookingDto.Email &&
                                             !b.Unbooked &&
                                             !b.Reserve &&
                                             e.Start > DateTime.Now
                                       select b).Count();

            if(countActiveBookings < 3)
            {
                var booking = new Booking
                {
                    Name = bookingDto.Name,
                    Email = bookingDto.Email,
                    Tel = bookingDto.Tel,
                    BookedAt = DateTime.Now,
                    EventId = bookingDto.EventId,
                    CancellationToken = Guid.NewGuid().ToString("N"),
                };

                if (bookingDto.Reserve)
                {
                    booking.Reserve = true;
                    booking.ReserveAt = DateTime.Now;
                }

                context.Bookings.Add(booking);
                context.SaveChanges();

                Console.WriteLine("Booking saved. Attempting to send email...");

                try
                {
                    var currentEvent = context.Events
                        .Where(e => e.Id == booking.EventId)
                        .FirstOrDefault();

                    var cancelUrl = $"https://aufgussapi-d6f4dae9g9h3a2h8.northeurope-01.azurewebsites.net/api/bookings/cancel-booking?token={booking.CancellationToken}";

                    var htmlMessage = $@"
                        <div style='font-family: Arial, sans-serif; font-size: 16px; color: #333;'>
                            <p><strong>Tack för din bokning!</strong></p>
        
                            <p>Du är inbokad till <strong>{currentEvent.Title}</strong> den {currentEvent.Start.ToString("yyyy-MM-dd HH:mm")} .</p>
        
                            <p>Om du vill avboka din plats, klicka på länken nedan:</p>
        
                            <p>
                                <a href='{cancelUrl}' style='color: #007bff; text-decoration: none;'>Avboka din plats</a>
                            </p>
        
                            <br>
        
                            <p>Vi ses på <strong>Freja Bad & Gym</strong>!</p>
                        </div>";
                    await _resendEmail.SendEmailAsync(
                            to: $"{bookingDto.Email}",
                            subject: "Bokningsbekräftelse Aufguss",
                            htmlContent: htmlMessage
                            //htmlContent: "<strong>Tack för din bokning!</strong><br>" +
                            //$"Du är inbokad till {eventTitle} <br>" +
                            //$"Vi ses på Freja Bad & Gym!"
                        );
                    return Ok();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Email sending failed: " + ex.Message);
                    return StatusCode(500, new { error = ex.Message });
                }

                return Ok(booking);
            }
            else
            {
                return BadRequest(new { message = "För många aktiva bokningar, max 3" });
            }



            
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

            var reserveList = context.Bookings.Where(b => b.EventId == booking.EventId && b.Reserve == true).OrderBy(b => b.ReserveAt).ToList();

            if (reserveList.Any())
            {
                reserveList[0].Reserve = false;
                reserveList[0].BookedAt = DateTime.Now;
            }

            context.SaveChanges();

            return Ok();
        }

    }

}
