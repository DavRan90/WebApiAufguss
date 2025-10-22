using Aufguss.Services.Interface;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace Aufguss.Services.API
{
    public class ApiBookingService : IBookingService
    {
        private readonly HttpClient _http;
        public ApiBookingService(HttpClient http) => _http = http;

        public Task<List<Booking>> GetBookingsAsync() =>
            _http.GetFromJsonAsync<List<Booking>>("api/bookings");
        public async Task<Booking?> GetBookingByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<Booking>($"api/Bookings/{id}");
        }

        public async Task<List<Booking>> AddBookingAsync(BookingDto BookingDtoData)
        {
            var response = await _http.PostAsJsonAsync("api/Bookings", BookingDtoData);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Booking>>()
                   ?? new List<Booking>(); // fallback in case of null
        }
        public async Task EditBookingAsync(int id, BookingDto bookingDto)
        {
            var response = await _http.PutAsJsonAsync($"api/bookings/{id}", bookingDto);
            response.EnsureSuccessStatusCode();
        }
        public async Task UnbookAsync(int id)
        {
            var response = await _http.PutAsync("api/Bookings/Unbook/" + id, null);
            response.EnsureSuccessStatusCode();
        }
        public async Task RemoveBookingAsync(int id)
        {
            var response = await _http.DeleteAsync("api/bookings/" + id);
            response.EnsureSuccessStatusCode();
        }
    }
}
