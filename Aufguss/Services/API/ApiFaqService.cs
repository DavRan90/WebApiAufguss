using Aufguss.Models;
using Aufguss.Services.Interface;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace Aufguss.Services.API
{
    public class ApiFaqService : IFaqService
    {
        private readonly HttpClient _http;
        public ApiFaqService(HttpClient http) => _http = http;

        public Task<List<Faq>> GetFaqsAsync() =>
            _http.GetFromJsonAsync<List<Faq>>("api/Faqs");

        public Task<Faq> GetFaqAsync(int id)
        {
            var faq = _http.GetFromJsonAsync<Faq>($"api/Faqs/{id}");
            return faq;
        }

        public async Task<Faq> AddFaqAsync(FaqDto FaqDtoData)
        {
            var response = await _http.PostAsJsonAsync("api/Faqs", FaqDtoData);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Faq>();
        }
        public async Task EditFaqAsync(int id, FaqDto faqDto)
        {
            var response = await _http.PutAsJsonAsync($"api/Faqs/{id}", faqDto);
            response.EnsureSuccessStatusCode();
        }
        public async Task RemoveFaqAsync(int id)
        {
            var response = await _http.DeleteAsync("api/faqs/" + id);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateFaqOrderAsync(List<Models.Faq> reorderedFaqs)
        {
            // Only send Id + Position to minimize payload
            var payload = reorderedFaqs.Select(f => new { f.Id, f.Position }).ToList();
            var response = await _http.PostAsJsonAsync("api/faqs/update-order", payload);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to update FAQ order.");
            }
        }
    }
}
    

