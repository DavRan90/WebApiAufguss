using Aufguss.Models;
using Blazored.LocalStorage;
using global::Aufguss.Services.Interface;
using System.Net.Http.Json;

namespace Aufguss.Services.API
{
    

    public class ApiRefreshService : IRefreshService
    {
        private readonly HttpClient _client;
        private readonly ILocalStorageService _storage;

        public ApiRefreshService(IHttpClientFactory factory, ILocalStorageService storage)
        {
            _client = factory.CreateClient("AuthenticatedClient");
            _storage = storage;
        }

        public async Task<bool> TryRefreshAsync()
        {
            var refreshToken = await _storage.GetItemAsync<string>("refreshToken");
            if (refreshToken is null)
                return false;

            var response = await _client.PostAsJsonAsync("/auth/refresh-token", new { RefreshToken = refreshToken });

            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            if (result is null)
                return false;

            // Store new tokens
            await _storage.SetItemAsync("token", result.AccessToken);
            await _storage.SetItemAsync("refreshToken", result.RefreshToken);

            return true;
        }
    }

}
