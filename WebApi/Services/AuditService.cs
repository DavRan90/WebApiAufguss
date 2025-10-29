using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApi.Models;

namespace WebApi.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetCurrentUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email)
                ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        }

        public async Task LogBookingAsync(int bookingId, string action, string? userEmail, string? details = null)
        {
            var booking = await _context.Bookings.Include(b => b.Event).FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null) return;

            var actorEmail = GetCurrentUserEmail() ?? userEmail ?? "Okänd användare";

            // Log booking audit
            var bookingAudit = new BookingAudit
            {
                BookingId = booking.Id,
                Action = action,
                UserEmail = actorEmail,
                Timestamp = DateTime.UtcNow,
                Details = details
            };
            _context.BookingAudits.Add(bookingAudit);

            // Also log corresponding event audit
            var eventAudit = new EventAudit
            {
                EventId = booking.EventId,
                Action = action,
                UserEmail = actorEmail,
                Timestamp = DateTime.UtcNow,
                Details = details ?? $"Deltagare {booking.Email} {action.ToLower()} sin plats till eventet '{booking.Event.Title}'."
            };
            _context.EventAudits.Add(eventAudit);

            await _context.SaveChangesAsync();
        }

        public async Task LogEventAsync(int eventId, string action, string? userEmail, string? details = null)
        {
            var actorEmail = GetCurrentUserEmail() ?? userEmail ?? "Okänd användare";

            var audit = new EventAudit
            {
                EventId = eventId,
                Action = action,
                UserEmail = actorEmail,
                Timestamp = DateTime.UtcNow,
                Details = details
            };

            _context.EventAudits.Add(audit);
            await _context.SaveChangesAsync();
        }
    }
}
