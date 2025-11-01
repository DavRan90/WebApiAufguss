using Aufguss.Models;
using Aufguss.Services.Interface;

namespace Aufguss.Services.Demo
{
    public class DemoLogService : ILogService
    {
        private static readonly string[] BookingActions = new[]
        {
            "Created Booking", "Updated Booking", "Cancelled Booking", "Checked In", "Checked Out"
        };

        private static readonly string[] EventActions = new[]
        {
            "Created Event", "Updated Event", "Cancelled Event", "Published Event", "Archived Event"
        };

        private static readonly string[] Users = new[]
        {
            "alice@example.com", "bob@example.com", "carol@example.com", "dave@example.com"
        };

        private readonly List<BookingAudit> _bookingAudits;
        private readonly List<EventAudit> _eventAudits;

        public DemoLogService()
        {
            _bookingAudits = GenerateBookingAudits();
            _eventAudits = GenerateEventAudits();
        }

        public Task<List<BookingAudit>> GetBookingAuditsAsync()
        {
            return Task.FromResult(_bookingAudits.OrderByDescending(l => l.Timestamp).ToList());
        }

        public Task<List<EventAudit>> GetEventAuditsAsync()
        {
            return Task.FromResult(_eventAudits.OrderByDescending(l => l.Timestamp).ToList());
        }

        private static List<BookingAudit> GenerateBookingAudits() =>
            Enumerable.Range(1, 25).Select(i => new BookingAudit
            {
                Id = i,
                Timestamp = DateTime.UtcNow.AddMinutes(-i * 5),
                Action = BookingActions[Random.Shared.Next(BookingActions.Length)],
                UserEmail = Users[Random.Shared.Next(Users.Length)],
                Details = $"Booking #{Random.Shared.Next(1000, 2000)} action performed."
            }).ToList();

        private static List<EventAudit> GenerateEventAudits() =>
            Enumerable.Range(1, 25).Select(i => new EventAudit
            {
                Id = i,
                Timestamp = DateTime.UtcNow.AddMinutes(-i * 7),
                Action = EventActions[Random.Shared.Next(EventActions.Length)],
                UserEmail = Users[Random.Shared.Next(Users.Length)],
                Details = $"Event #{Random.Shared.Next(500, 1500)} action performed."
            }).ToList();
    }
}
