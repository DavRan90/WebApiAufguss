using WebApi.Models;

namespace WebApi.Services
{
    public interface IBookingManager
    {
        Task<(bool Success, string Message)> CreateBookingAsync(BookingDto dto);
        Task<(bool Success, string Message)> UnbookAsync(int id, string performedBy, bool isAdmin = false);
        Task<(bool Success, string Message)> DeleteBookingAsync(int bookingId);
        Task<(bool Success, string Message, Booking? Booking)> EditBookingAsync(int id, BookingDto dto);
        Task<(bool Success, string Message)> CancelBookingByTokenAsync(string token);
        Task CheckReserveListAsync(Booking booking);
    }
}
