using Aufguss.Models;
using Aufguss.Services.Interface;
using System.Net.Http.Json;

namespace Aufguss.Services.API
{
    public class ApiTemplateService : ITemplateService
    {
        private readonly HttpClient _http;
        public ApiTemplateService(HttpClient http) => _http = http;

        public Task<List<EventTemplate>> GetTemplatesAsync() =>
            _http.GetFromJsonAsync<List<EventTemplate>>("api/EventTemplates");
        public async Task<EventTemplate?> GetTemplateByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<EventTemplate>($"api/EventTemplates/{id}");
        }
        public async Task<EventTemplate> CreateTemplateAsync(EventDto templateDto)
        {
            var response = await _http.PostAsJsonAsync("api/EventTemplates", templateDto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                var message = error != null && error.ContainsKey("message")
                    ? error["message"]
                    : "Ett okänt fel uppstod vid bokning.";

                throw new Exception(message);
            }

            return await response.Content.ReadFromJsonAsync<EventTemplate>()
                   ?? throw new Exception("Failed to deserialize booking");
        }
        public async Task DeleteTemplateAsync(int id)
        {
            var response = await _http.DeleteAsync("api/EventTemplates/" + id);
            response.EnsureSuccessStatusCode();
        }

    }
}
