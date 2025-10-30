using Aufguss.Models;
using Aufguss.Pages;
using Aufguss.Services.Interface;
using System.Xml.Linq;
using static Aufguss.Pages.BookAufguss;
using static System.Net.WebRequestMethods;

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
                Email = "david.rantanen@telia.com",
                Tel = "0701234567",
                BookedAt = DateTime.Now,
                Unbooked = true,
                Reserve = false,
                EventId = 1,
            },
            new Booking {
                Id = 1,
                Name = "Demo",
                Email = "david.rantanen@telia.com",
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
        public async Task<List<BookingStatsDto>> GetPublicBookingStatsAsync()
        {
            return new List<BookingStatsDto>();
        }

        public Task<List<Booking>> GetBookingsAsync()
        {
            return Task.FromResult(_bookings);
        }

        public async Task<Booking> AddBookingAsync(BookingDto bookingDto)
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

            return newBooking;
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

        public async Task ConfirmAttendanceAsync(int bookingId, double userLat, double userLng)
        {
            await EnsureEventsLoadedAsync();
            await EnsureSettingsLoadedAsync();

            var booking = _bookings.SingleOrDefault(b => b.Id == bookingId);
            if (booking == null)
                throw new Exception($"Bokning med ID {bookingId} hittades inte.");

            var evt = _events.FirstOrDefault(e => e.Id == booking.EventId);
            if (evt == null)
                throw new Exception($"Event med ID {booking.EventId} hittades inte.");

            // 🕒 1. Check time constraint (within 2 hours of start)
            var now = DateTime.Now;
            var timeUntilStart = evt.Start - now;
            if (timeUntilStart > TimeSpan.FromHours(2))
                throw new Exception("För tidigt att bekräfta närvaro (mer än 2 timmar kvar till start).");

            // 📍 2. Check geolocation constraint (within 500 m)
            double eventLat = 59.04406605697291;   // Freja bad & gym
            double eventLng = 17.3156782887916;
            
            var distanceMeters = CalculateDistanceMeters(userLat, userLng, eventLat, eventLng);
            if (distanceMeters > 500)
                throw new Exception($"Du är för långt bort ({Math.Round(distanceMeters)} m). Max 500 m tillåtet.");

            // ✅ 3. Mark as confirmed
            booking.AttendanceConfirmed = true;
            booking.AttendanceConfirmedAt = DateTime.Now;

            Console.WriteLine($"[DEMO] {booking.Name} bekräftade närvaro för event '{evt.Title}' ({Math.Round(distanceMeters)} m bort).");

            await Task.CompletedTask; // Demo only; no DB persistence
        }

        // 🔢 Helper to calculate the distance between two coordinates in meters
        private static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Earth radius in meters
            double dLat = ToRadians(lat2 - lat1);
            double dLon = ToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRadians(double deg) => deg * (Math.PI / 180);
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
