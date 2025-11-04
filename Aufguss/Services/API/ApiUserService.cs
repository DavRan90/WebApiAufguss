using Aufguss.Models;
using Aufguss.Services.Interface;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace Aufguss.Services.API
{
    public class ApiUserService : IUserService
    {
        //private readonly HttpClient _http;
        //public ApiUserService(HttpClient http) => _http = http;
        private readonly HttpClient _http;

        public ApiUserService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("AuthenticatedClient");
        }

        
            
        public async Task<List<UserWithRolesDto>> GetUsersAsync()
        {
            return await _http.GetFromJsonAsync<List<UserWithRolesDto>>("api/users");
        }

        public async Task<UserWithRolesDto?> GetUserAsync(string? id, string? phoneNumber)
        {
            return await _http.GetFromJsonAsync<UserWithRolesDto>($"api/user/{id}");
        }
        public async Task<UserWithRolesDto?> GetCurrentUserAsync()
        {
            return await _http.GetFromJsonAsync<UserWithRolesDto>("api/users/me");
        }
        public async Task<List<UserWithRolesDto>> GetMyFriendsAsync()
        {
            return await _http.GetFromJsonAsync<List<UserWithRolesDto>>("api/users/me/friends");
        }
        public async Task EditUserAsync(string id, UserWithRolesDto userDto)
        {
            var response = await _http.PutAsJsonAsync($"api/Users/{id}", userDto);
            response.EnsureSuccessStatusCode();
        }
        public async Task RemoveUserAsync(string id)
        {
            var response = await _http.DeleteAsync("api/users/" + id);
            response.EnsureSuccessStatusCode();
        }
    }
}
