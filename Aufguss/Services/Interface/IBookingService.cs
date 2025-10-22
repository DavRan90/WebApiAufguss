using static System.Net.WebRequestMethods;

namespace Aufguss.Services.Interface
{
    public interface IBookingService
    {
        Task<List<Booking>> GetBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<List<Booking>> AddBookingAsync(BookingDto bookingDto);
        Task EditBookingAsync(int id, BookingDto bookingDto);
        Task UnbookAsync(int id);
        Task RemoveBookingAsync(int id);
    }
}
