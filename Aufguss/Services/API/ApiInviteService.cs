using Aufguss.Models;
using Aufguss.Services.Interface;
using System.Net.Http.Json;

namespace Aufguss.Services.API
{
    public class ApiInviteService : IInviteService
    {
        private readonly HttpClient _http;
        public ApiInviteService(HttpClient http) => _http = http;

        public Task<List<InviteDto>> GetInvitesAsync(string url) =>
            _http.GetFromJsonAsync<List<InviteDto>>(url);
    }
}
