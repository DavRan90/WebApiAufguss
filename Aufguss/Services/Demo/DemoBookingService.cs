using Aufguss.Models;
using Aufguss.Pages;
using Aufguss.Services.Interface;
using System.Xml.Linq;

namespace Aufguss.Services.Demo
{
    public class DemoBookingService : IBookingService
    {
        
        private readonly List<Booking> _bookings;
        private SiteSettings _settings;

        private readonly ISettingsService _settingsService;
        private readonly IEventService _eventService;


        public DemoBookingService(ISettingsService settingsService, IEventService eventService)
        {
            _settings = null;
            _settingsService = settingsService;
            _eventService = eventService;

            _bookings = new List<Booking>
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
        }
        private List<Event> _events;

        private async Task EnsureEventsLoadedAsync()
        {
            if (_events == null)
            {
                _events = await _eventService.GetEventsAsync();
            }
        }
        private async Task EnsureSettingsLoadedAsync()
        {
            if (_settings == null)
            {
                _settings = await _settingsService.GetSettingsAsync();
            }
        }

        public Task<List<Booking>> GetBookingsAsync()
        {
            return Task.FromResult(_bookings);
        }

        public async Task<List<Booking>> AddBookingAsync(BookingDto bookingDto)
        {
            _settings = await _settingsService.GetSettingsAsync();
            await EnsureEventsLoadedAsync();
            await EnsureSettingsLoadedAsync();

            var bookingEvent = _events.FirstOrDefault(e => e.Id == bookingDto.EventId);
            int maxSlots = bookingEvent?.MaxSlots ?? 3;

            // 2. Count active bookings for this user
            int countActiveBookings = _bookings.Count(b =>
                b.Email == bookingDto.Email &&
                !b.Unbooked &&
                !b.Reserve &&
                // Here you could filter by event date > now if you have event info, else skip
                true);

            // 3. Check against max allowed bookings
            if (countActiveBookings >= _settings.MaxBookingsAufguss)
            {
                
                throw new Exception($"För många aktiva bokningar, max {_settings.MaxBookingsAufguss}");
            }

            // 4. Check event max slots and existing bookings for that event

            int existingBookings = _bookings.Count(b =>
                b.EventId == bookingDto.EventId &&
                !b.Unbooked &&
                !b.Reserve);

            bool isReserve = existingBookings >= maxSlots;

            // 5. Create booking
            var newBooking = new Booking
            {
                Id = _bookings.Any() ? _bookings.Max(b => b.Id) + 1 : 1,
                UserId = bookingDto.UserId,
                Name = bookingDto.Name,
                Email = bookingDto.Email,
                Tel = bookingDto.Tel,
                BookedAt = DateTime.Now,
                Reserve = isReserve,
                EventId = bookingDto.EventId,
            };

            _bookings.Add(newBooking);

            return _bookings.OrderByDescending(b => b.Id).ToList();
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
                booking.UnbookedAt = DateTime.Now;

                CheckReserveList(booking);
            }

            return Task.CompletedTask;
        }

        public Task RemoveBookingAsync(int id)
        {
            var booking = _bookings.SingleOrDefault(b => b.Id == id);
            if (booking != null)
            {
                _bookings.Remove(booking);
                CheckReserveList(booking);
            }

            return Task.CompletedTask;
        }

        public Task<Booking?> GetBookingByIdAsync(int id)
        {
            var booking = _bookings.SingleOrDefault(b => b.Id == id);
            return Task.FromResult(booking);
        }
        private void CheckReserveList(Booking booking)
        {
            if (_events == null || !_events.Any())
            {
                Console.WriteLine("[DEMO] No events loaded — skipping reserve check.");
                return;
            }

            var evt = _events.FirstOrDefault(e => e.Id == booking.EventId);
            if (evt == null)
            {
                Console.WriteLine($"[DEMO] Event with ID {booking.EventId} not found — skipping reserve check.");
                return;
            }

            int activeCount = _bookings.Count(b =>
                b.EventId == evt.Id &&
                !b.Unbooked &&
                !b.Reserve);

            if (activeCount < evt.MaxSlots)
            {
                var reserveList = _bookings
                    .Where(b => b.EventId == evt.Id && b.Reserve && !b.Unbooked)
                    .OrderBy(b => b.ReserveAt)
                    .ToList();

                if (reserveList.Any())
                {
                    var promote = reserveList.First();
                    promote.Reserve = false;
                    promote.BookedAt = DateTime.Now;
                    promote.ReserveAt = null;

                    Console.WriteLine($"[DEMO] Promoted {promote.Name} from reserve list for event {evt.Title}");
                }
            }
        }

    }
}
