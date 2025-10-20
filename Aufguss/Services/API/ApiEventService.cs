using Aufguss.Services.Interface;
using System.Net.Http.Json;

namespace Aufguss.Services.API
{
    public class ApiEventService : IEventService
    {
        private readonly HttpClient _http;
        public ApiEventService(HttpClient http) => _http = http;

        public Task<List<Event>> GetEventsAsync() =>
            _http.GetFromJsonAsync<List<Event>>("api/events");
        public async Task<Event?> GetEventByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<Event>($"api/Events/{id}");
        }

    }
}
