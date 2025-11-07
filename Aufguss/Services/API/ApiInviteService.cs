using Aufguss.Models;
using Aufguss.Services.Interface;
using System;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace Aufguss.Services.API
{
    public class ApiInviteService : IInviteService
    {
        //private readonly HttpClient _http;
        //public ApiInviteService(HttpClient http) => _http = http;
        private readonly HttpClient _http;

        public ApiInviteService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("AuthenticatedClient");
        }

        public Task<List<InviteDto>> GetInvitesAsync() =>
            _http.GetFromJsonAsync<List<InviteDto>>("api/invites/");
        public async Task<List<InviteDto>> GetInviterAsync(string id)
        {
            return await _http.GetFromJsonAsync<List<InviteDto>>($"api/invites/inviters?inviterId={id}");

            //return Task.FromResult(inviters);
        }
        public async Task<List<InviteDto>> GetInviteeAsync(string id)
        {
            return await _http.GetFromJsonAsync<List<InviteDto>>($"api/invites/invitees?inviteeId={id}");
        }
        public async Task SendInviteByPhoneAsync(string phoneNumber)
        {
            var request = new { PhoneNumber = phoneNumber };
            var response = await _http.PostAsJsonAsync("api/invites/send-by-phone", request);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error sending invite: {content}");
            }
        }
        public async Task<List<InviteDto>> AddInviteAsync(InviteDto inviteDtoData)
        {
            var response = await _http.PostAsJsonAsync("api/invites", inviteDtoData);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<InviteDto>>()
                   ?? new List<InviteDto>(); // fallback in case of null
        }
        public async Task RemoveInviteAsync(int id)
        {
            var response = await _http.DeleteAsync("api/invites/" + id);
            response.EnsureSuccessStatusCode();
        }
        public async Task<string> AcceptInviteAsync(int id, InviteDto dto)
        {
            var response = await _http.PutAsync($"api/Invites/{id}", null);

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsStringAsync();

            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Failed to accept invite: {error}");
        }
    }
}
