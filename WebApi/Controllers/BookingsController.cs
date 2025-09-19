using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public BookingsController(ApplicationDbContext context)
        {
            this.context = context;
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

        [HttpPost]
        public IActionResult CreateBooking(BookingDto bookingDto)
        {

            var booking = new Booking
            {
                Name = bookingDto.Name,
                Email = bookingDto.Email,
                Tel = bookingDto.Tel,
                BookedAt = DateTime.Now,
                EventId = bookingDto.EventId,
            };

            if(bookingDto.Reserve == true)
            {
                booking.Reserve = true;
                booking.ReserveAt = DateTime.Now;
            }

            context.Bookings.Add(booking);
            context.SaveChanges();

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
            if(bookingDto.Unbooked == true)
            {
                booking.Unbooked = true;
                booking.UnbookedAt = DateTime.Now;
                var reserveList = context.Bookings.Where(b => b.Reserve == true).OrderBy(b => b.ReserveAt).ToList();

                if (reserveList?.Any() == true)
                {
                    reserveList[0].Reserve = false;
                    reserveList[0].BookedAt = DateTime.Now;
                }
            }
            else
            {
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
            var reserveList = context.Bookings.Where(b => b.Reserve == true).OrderBy(b => b.ReserveAt).ToList();

            if(reserveList != null)
            {
                reserveList[0].Reserve = false;
                reserveList[0].BookedAt = DateTime.Now;
            }

            context.SaveChanges();

            return Ok();
        }

    }

}
