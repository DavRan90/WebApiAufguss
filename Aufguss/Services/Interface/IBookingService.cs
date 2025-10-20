namespace Aufguss.Services.Interface
{
    public interface IBookingService
    {
        Task<List<Booking>> GetBookingsAsync();
    }
}
