using Aufguss.Models;
using Aufguss.Services.Interface;

namespace Aufguss.Services.Demo
{
    public class DemoBookingService : IBookingService
    {
        public Task<List<Booking>> GetBookingsAsync()
        {
            return Task.FromResult(new List<Booking>
        {
            new Booking {
                Name = "Namn",
                Email = "Email@test.se",
                Tel = "0700000000",
                BookedAt = DateTime.Now,
                Unbooked = true,
                Reserve = false,
                EventId = 1,
            },
            new Booking {
                Name = "Namn",
                Email = "Email@test.se",
                Tel = "0700000000",
                BookedAt = DateTime.Now,
                Unbooked = false,
                Reserve = false,
                EventId = 1,
            },
            new Booking {
                Name = "Namn",
                Email = "Email@test.se",
                Tel = "0700000000",
                BookedAt = DateTime.Now,
                Unbooked = false,
                Reserve = false,
                EventId = 1,
            },
            new Booking {
                Name = "Namn",
                Email = "Email@test.se",
                Tel = "0700000000",
                BookedAt = DateTime.Now,
                Unbooked = false,
                Reserve = false,
                EventId = 1,
            },
            new Booking {
                Name = "Namn",
                Email = "Email@test.se",
                Tel = "0700000000",
                BookedAt = DateTime.Now,
                Unbooked = false,
                Reserve = false,
                EventId = 1,
            },
            new Booking {
                Name = "Namn",
                Email = "Email@test.se",
                Tel = "0700000000",
                BookedAt = DateTime.Now,
                Unbooked = false,
                Reserve = false,
                EventId = 1,
            },
            new Booking {
                Name = "Namn",
                Email = "Email@test.se",
                Tel = "0700000000",
                BookedAt = DateTime.Now,
                Unbooked = false,
                Reserve = true,
                EventId = 1,
            }
        });
        }
    }
}
