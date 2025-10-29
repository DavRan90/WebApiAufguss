namespace WebApi.Services
{
    public interface IAuditService
    {
        Task LogBookingAsync(int bookingId, string action, string? userEmail, string? details = null);
        Task LogEventAsync(int eventId, string action, string? userEmail, string? details = null);
    }

}
