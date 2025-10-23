using Aufguss.Models;
using Aufguss.Services.Interface;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace Aufguss.Services.API
{
    public class ApiImageService : IImageService
    {
        private readonly HttpClient _http;

        public ApiImageService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<string>> GetImageListAsync()
        {
            return await _http.GetFromJsonAsync<List<string>>("api/upload/news");
        }
    }
}
