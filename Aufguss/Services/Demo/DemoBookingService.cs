using Aufguss.Models;
using Aufguss.Services.Interface;
using System.Xml.Linq;

namespace Aufguss.Services.Demo
{
    public class DemoBookingService : IBookingService
    {
        private readonly List<Booking> _bookings = new()
    {
            new Booking {
                Id = 0,
                Name = "Demo",
                UserId = "demo-user-id",
                Email = "demo@demo.com",
                Tel = "0701234567",
                BookedAt = DateTime.Now,
                Unbooked = true,
                Reserve = false,
                EventId = 1,
            },
            new Booking {
                Id = 1,
                Name = "Demo",
                Email = "demo@demo.com",
                UserId = "demo-user-id",
                Tel = "0701234567",
                BookedAt = DateTime.Now,
                Unbooked = false,
                Reserve = false,
                EventId = 3,
            },
            new Booking {
                Id = 2,
                Name = "Alice Andersson",
                UserId = "user-001",
                Email = "alice@example.com",
                Tel = "0701000001",
                BookedAt = DateTime.Now,
                Unbooked = true,
                Reserve = false,
                EventId = 2,
            },
            new Booking {
                Id = 3,
                Name = "Björn Berg",
                Email = "bjorn@example.com",
                Tel = "0701000002",
                BookedAt = DateTime.Now,
                Unbooked = false,
                Reserve = false,
                EventId = 1,
            },
            new Booking {
                Id = 4,
                Name = "Carina Nilsson",
                Email = "carina@demo.com",
                UserId = "2d9403fe-07a3-486a-9315-90ef770e013b",
                Tel = "0701000003",
                BookedAt = DateTime.Now,
                Unbooked = false,
                Reserve = false,
                EventId = 2,
            },
            new Booking {
                Id = 5,
                Name = "David Dahl",
                Email = "david@example.com",
                Tel = "0701000004",
                BookedAt = DateTime.Now,
                Unbooked = false,
                Reserve = false,
                EventId = 1,
            },
            new Booking {
                Id = 6,
                Name = "Elin Ek",
                Email = "elin@example.com",
                Tel = "0701000005",
                BookedAt = DateTime.Now,
                Unbooked = false,
                Reserve = false,
                EventId = 2,
            },
            new Booking {
                Id = 7,
                Name = "Fredrik Frisk",
                Email = "fredrik@example.com",
                Tel = "0701000006",
                BookedAt = DateTime.Now,
                Unbooked = false,
                Reserve = false,
                EventId = 1,
            },
            new Booking {
                Id = 8,
                Name = "Greta Gran",
                Email = "greta@example.com",
                Tel = "0701000007",
                BookedAt = DateTime.Now,
                Unbooked = false,
                Reserve = true,
                EventId = 2,
}
        };

        public Task<List<Booking>> GetBookingsAsync()
        {
            return Task.FromResult(_bookings);
        }

        public Task<List<Booking>> AddBookingAsync(BookingDto bookingDto)
        {
            var newBooking = new Booking
            {
                Id = _bookings.Any() ? _bookings.Max(b => b.Id) + 1 : 1,
                UserId = bookingDto.UserId,
                Name = bookingDto.Name,
                Email = bookingDto.Email,
                Tel = bookingDto.Tel,
                BookedAt = DateTime.Now,
                Reserve = bookingDto.Reserve,
                EventId = bookingDto.EventId,
            };

            _bookings.Add(newBooking);
            return Task.FromResult(_bookings.OrderByDescending(b => b.Id).ToList());
        }

        public Task EditBookingAsync(int id, BookingDto bookingDto)
        {
            var existing = _bookings.FirstOrDefault(b => b.Id == id);
            if (existing == null)
                throw new Exception($"FAQ med ID {id} hittades inte.");

            existing.Name = bookingDto.Name;
            existing.Email = bookingDto.Email;
            existing.Tel = bookingDto.Tel;
            existing.BookedAt = DateTime.Now;
            existing.Reserve = bookingDto.Reserve;
            existing.Unbooked = bookingDto.Unbooked ?? false;
            existing.EventId = bookingDto.EventId;

            return Task.CompletedTask;
        }
        public Task UnbookAsync(int id)
        {
            var booking = _bookings.SingleOrDefault(b => b.Id == id);
            if (booking != null)
            {
                booking.Unbooked = true;
            }

            return Task.CompletedTask;
        }

        public Task RemoveBookingAsync(int id)
        {
            var booking = _bookings.SingleOrDefault(b => b.Id == id);
            if (booking != null)
            {
                _bookings.Remove(booking);
            }

            return Task.CompletedTask;
        }

        public Task<Booking?> GetBookingByIdAsync(int id)
        {
            var booking = _bookings.SingleOrDefault(b => b.Id == id);
            return Task.FromResult(booking);
        }

    }
}
