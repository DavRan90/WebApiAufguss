using Aufguss.Services.Interface;
using System.Net.Http.Json;

namespace Aufguss.Services.API
{
    public class ApiEntryService : IEntryService
    {
        private readonly HttpClient _http;
        public ApiEntryService(HttpClient http) => _http = http;

        public Task<List<Entry>> GetEntriesAsync() =>
            _http.GetFromJsonAsync<List<Entry>>("api/entries");
    }
}
