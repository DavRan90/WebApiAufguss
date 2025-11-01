using System.Net.Http.Json;
using Aufguss.Models;
using Aufguss.Services.Interface;
using static Aufguss.Pages.Logs;

namespace Aufguss.Services.API
{
    public class ApiLogService : ILogService
    {
        private readonly HttpClient _http;

        public ApiLogService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<BookingAudit>> GetBookingAuditsAsync()
        {
            var response = await _http.GetFromJsonAsync<CombinedAuditLogsDto>("api/admin/audit-logs");
            return response?.Bookings?.Select(MapBookingAuditDto)?.ToList() ?? new List<BookingAudit>();
        }

        public async Task<List<EventAudit>> GetEventAuditsAsync()
        {
            var response = await _http.GetFromJsonAsync<CombinedAuditLogsDto>("api/admin/audit-logs");
            return response?.Events?.Select(MapEventAuditDto)?.ToList() ?? new List<EventAudit>();
        }

        // Optional mapping methods in case your DTOs differ slightly
        private static BookingAudit MapBookingAuditDto(AuditLogDto dto) => new()
        {
            Timestamp = dto.Timestamp,
            Action = dto.Action,
            UserEmail = dto.UserEmail,
            Details = dto.Details
        };

        private static EventAudit MapEventAuditDto(AuditLogDto dto) => new()
        {
            Timestamp = dto.Timestamp,
            Action = dto.Action,
            UserEmail = dto.UserEmail,
            Details = dto.Details
        };
    }
}
