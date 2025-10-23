using Aufguss.Models;
using Aufguss.Services.Interface;
using System.Net.Http.Json;

namespace Aufguss.Services.API
{
    public class ApiSettingsService : ISettingsService
    {
        private readonly HttpClient _http;
        public ApiSettingsService(HttpClient http) => _http = http;

        public Task<SiteSettings> GetSettingsAsync() =>
            _http.GetFromJsonAsync<SiteSettings>("api/settings");

        public async Task EditSettingsAsync(SiteSettings settingsDto)
        {
            var response = await _http.PutAsJsonAsync($"api/Settings", settingsDto);
            response.EnsureSuccessStatusCode();
        }
    }
}
