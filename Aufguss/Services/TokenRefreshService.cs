using System.Net.Http.Json;
using Aufguss.Models;
using Aufguss.Services.Interface;
using Blazored.LocalStorage;

namespace Aufguss.Services
{
    

    public class TokenRefreshService : IRefreshService
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _storage;

        public TokenRefreshService(HttpClient http, ILocalStorageService storage)
        {
            _http = http;
            _storage = storage;
        }

        public async Task<bool> TryRefreshAsync()
        {
            var refreshToken = await _storage.GetItemAsync<string>("refreshToken");
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var request = new RefreshTokenRequest { RefreshToken = refreshToken };

            HttpResponseMessage response;
            try
            {
                response = await _http.PostAsJsonAsync("api/auth/refresh-token", request);
            }
            catch
            {
                return false;
            }

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (result == null)
                return false;

            await _storage.SetItemAsync("token", result.AccessToken);
            await _storage.SetItemAsync("refreshToken", result.RefreshToken);

            return true;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            return await _storage.GetItemAsync<string>("token");
        }
    }

}
