using Aufguss.Models;

namespace Aufguss.Services.Interface
{
    public interface ILogService
    {
        Task<List<EventAudit>> GetEventAuditsAsync();
        Task<List<BookingAudit>> GetBookingAuditsAsync();
    }
}
