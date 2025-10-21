using Aufguss.Services.Interface;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace Aufguss.Services.API
{
    public class ApiEntryService : IEntryService
    {
        private readonly HttpClient _http;
        public ApiEntryService(HttpClient http) => _http = http;

        public Task<List<Entry>> GetEntriesAsync() =>
            _http.GetFromJsonAsync<List<Entry>>("api/entries");

        public async Task<List<Entry>> AddEntryAsync(EntryDto EntryDtoData)
        {
            var response = await _http.PostAsJsonAsync("api/Entries", EntryDtoData);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Entry>>()
                   ?? new List<Entry>(); // fallback in case of null
        }

        public async Task RemoveEntryAsync(int id)
        {
            var response = await _http.DeleteAsync("api/Entries/" + id);
            response.EnsureSuccessStatusCode();
        }
    }
}
