using Aufguss.Services.Interface;
using System.Net.Http.Json;

namespace Aufguss.Services.API
{
    public class ApiBookingService : IBookingService
    {
        private readonly HttpClient _http;
        public ApiBookingService(HttpClient http) => _http = http;

        public Task<List<Booking>> GetBookingsAsync() =>
            _http.GetFromJsonAsync<List<Booking>>("api/bookings");
    }
}
