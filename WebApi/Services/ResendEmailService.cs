using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebApi.Services
{
    
    public class ResendEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public ResendEmailService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["ResendSettings:ApiKey"] ?? throw new ArgumentNullException("Missing Resend API key");
        }

        public async Task SendEmailAsync(string to, string subject, string htmlContent)
        {
            var request = new
            {
                from = "onboarding@resend.dev", // Must match a verified domain (or use sandbox)
                to = new[] { to },
                subject = subject,
                html = htmlContent
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Resend failed: {response.StatusCode}, {error}");
            }
        }
    }

}
