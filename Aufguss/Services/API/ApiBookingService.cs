using Aufguss.Models;
using Aufguss.Services.Interface;
using System.Net.Http.Json;
using static Aufguss.Pages.BookAufguss;
using static System.Net.WebRequestMethods;

namespace Aufguss.Services.API
{
    public class ApiBookingService : IBookingService
    {
        private readonly HttpClient _http;

        public ApiBookingService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("AuthenticatedClient");
        }

        public async Task<List<BookingStatsDto>> GetPublicBookingStatsAsync()
        {
            var result = await _http.GetFromJsonAsync<List<BookingStatsDto>>("api/Bookings/public-stats");
            return result ?? new List<BookingStatsDto>();
        }

        public Task<List<Booking>> GetBookingsAsync() =>
            _http.GetFromJsonAsync<List<Booking>>("api/bookings");
        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<Booking>($"api/Bookings/{id}");
        }

        public async Task<Booking> AddBookingAsync(BookingDto BookingDtoData)
        {
            var response = await _http.PostAsJsonAsync("api/Bookings", BookingDtoData);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                var message = error != null && error.ContainsKey("message")
                    ? error["message"]
                    : "Ett okänt fel uppstod vid bokning.";

                throw new Exception(message);
            }

            return await response.Content.ReadFromJsonAsync<Booking>()
                   ?? throw new Exception("Failed to deserialize booking");
        }
        public async Task EditBookingAsync(int id, BookingDto bookingDto)
        {
            var response = await _http.PutAsJsonAsync($"api/bookings/{id}", bookingDto);
            response.EnsureSuccessStatusCode();
        }
        public async Task ConfirmAttendanceAsync(int bookingId, double userLat, double userLng)
        {
            
        }
        public async Task UnbookAsync(int id)
        {
            var response = await _http.PostAsync($"api/bookings/{id}/unbook", null);
            response.EnsureSuccessStatusCode();
        }
        public async Task RemoveBookingAsync(int id)
        {
            var response = await _http.DeleteAsync("api/bookings/" + id);
            response.EnsureSuccessStatusCode();
        }
    }
}
