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
            _http.GetFromJsonAsync<List<EventTemplate>>("api/templates");
        public async Task<EventTemplate?> GetTemplateByIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<EventTemplate>($"api/Templates/{id}");
        }

    }
}
