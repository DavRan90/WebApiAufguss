using Aufguss.Services.Interface;
using System.Net.Http.Json;

namespace Aufguss.Services
{
    public class FaqService
    {
        private readonly HttpClient _http;

        public FaqService(HttpClient http)
        {
            _http = http;
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
