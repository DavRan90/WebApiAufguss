using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;
using System.Runtime;

namespace WebApi.Services
{
    public class MailgunEmailService
    {
        private readonly MailgunSettings _settings;

        public MailgunEmailService(IOptions<MailgunSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string text)
        {
            var options = new RestClientOptions("https://api.mailgun.net")
            {
                Authenticator = new HttpBasicAuthenticator("api", _settings.ApiKey)
            };

            var client = new RestClient(options);

            var request = new RestRequest($"/v3/{_settings.Domain}/messages", Method.Post);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("from", _settings.From);
            request.AddParameter("to", to);
            request.AddParameter("subject", subject);
            request.AddParameter("text", text);

            var response = await client.ExecuteAsync(request);

            if (!response.IsSuccessful)
            {
                Console.WriteLine("Mailgun error:");
                Console.WriteLine(response.Content);
            }

            return response.IsSuccessful;
        }
    }
}
