using Aufguss.Services.Interface;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace Aufguss.Services.API
{
    public class ApiEntryService : IEntryService
    {
        //private readonly HttpClient _http;
        //public ApiEntryService(HttpClient http) => _http = http;
        private readonly HttpClient _http;

        public ApiEntryService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("AuthenticatedClient");
        }

        public Task<List<Entry>> GetEntriesAsync() =>
            _http.GetFromJsonAsync<List<Entry>>("api/entries");

        public async Task<Entry?> AddEntryAsync(EntryDto entryDtoData)
        {
            var response = await _http.PostAsJsonAsync("api/Entries", entryDtoData);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Entry>();
        }

        public async Task RemoveEntryAsync(int id)
        {
            var response = await _http.DeleteAsync("api/Entries/" + id);
            response.EnsureSuccessStatusCode();
        }
    }
}
