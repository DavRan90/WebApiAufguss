using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace WebApi.Services
{
    public class ResendEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<ResendEmailService> _logger;

        public ResendEmailService(HttpClient httpClient, IConfiguration config, ILogger<ResendEmailService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = config["ResendSettings:ApiKey"] ?? throw new ArgumentNullException("Missing Resend API key");
        }

        public async Task<string> SendEmailAsync(string to, string subject, string htmlContent)
        {
            var request = new
            {
                from = "noreply@frejabadogym.se",
                to = new[] { to },
                subject,
                html = htmlContent
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Resend failed: {response.StatusCode}, {responseBody}");

            return responseBody; // return body for inspection
        }

        public async Task<string> SendTestEmailAsync(string to, string subject, string htmlContent)
        {
            var request = new
            {
                from = "noreply@frejabadogym.se",
                to = new[] { to },
                subject,
                html = htmlContent
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
            {
                Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.SendAsync(httpRequest);
            return await response.Content.ReadAsStringAsync();
        }


    }
}
