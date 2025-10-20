using Aufguss.Models;
using Aufguss.Services.Interface;
using System.Net.Http.Json;

namespace Aufguss.Services.API
{
    public class ApiUserService : IUserService
    {
        private readonly HttpClient _http;
        public ApiUserService(HttpClient http) => _http = http;

        public Task<List<UserWithRolesDto>> GetUsersAsync() =>
            _http.GetFromJsonAsync<List<UserWithRolesDto>>("api/auth/users");

        public async Task<UserWithRolesDto?> GetUserByIdAsync(string id)
        {
            return await _http.GetFromJsonAsync<UserWithRolesDto>($"api/user/{id}");
        }
    }
}
