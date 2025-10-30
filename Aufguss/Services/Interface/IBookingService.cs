using Aufguss.Models;
using static Aufguss.Pages.BookAufguss;
using static System.Net.WebRequestMethods;

namespace Aufguss.Services.Interface
{
    public interface IBookingService
    {
        Task<List<Booking>> GetBookingsAsync();
        Task<Booking?> GetBookingByIdAsync(int id);
        Task<Booking> AddBookingAsync(BookingDto bookingDto);
        Task EditBookingAsync(int id, BookingDto bookingDto);
        Task ConfirmAttendanceAsync(int bookingId, double userLat, double userLng);
        Task UnbookAsync(int id);
        Task RemoveBookingAsync(int id);
        Task<List<BookingStatsDto>> GetPublicBookingStatsAsync();
    }
}
