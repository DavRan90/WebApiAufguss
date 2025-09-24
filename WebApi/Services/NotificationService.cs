using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.Extensions.Options;

namespace WebApi.Services
{
    public class NotificationService
    {

        private readonly string _apiKey;

        public NotificationService(IConfiguration config)
        {
            // Read from appsettings.json or environment variable
            _apiKey = config["SendGridSettings:ApiKey"] ?? Environment.GetEnvironmentVariable("SendGridSettings");
        }

        public async Task SendEmailAsync(string toEmail, string subject, string plainText, string htmlContent)
        {
            var options = new SendGridClientOptions
            {
                ApiKey = _apiKey
            };
            options.SetDataResidency("eu");

            var client = new SendGridClient(options);

            var from = new EmailAddress("noreply@yourdomain.com", "Freja Bad & Gym"); // replace with your actual from address
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlContent);

            var response = await client.SendEmailAsync(msg);

            // Optional: check result
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync();
                throw new Exception($"SendGrid failed: {response.StatusCode}, {body}");
            }
        }
    }
}
