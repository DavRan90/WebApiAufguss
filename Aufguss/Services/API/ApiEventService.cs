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

        public async Task<List<Event>> AddEventAsync(EventDto EventDtoData)
        {
            var response = await _http.PostAsJsonAsync("api/Events", EventDtoData);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Event>>()
                   ?? new List<Event>(); // fallback in case of null
        }
        public async Task EditEventAsync(int id, EventDto eventDto)
        {
            var response = await _http.PutAsJsonAsync($"api/events/{id}", eventDto);
            response.EnsureSuccessStatusCode();
        }
        public async Task RemoveEventAsync(int id)
        {
            var response = await _http.DeleteAsync("api/events/" + id);
            response.EnsureSuccessStatusCode();
        }

    }
}
