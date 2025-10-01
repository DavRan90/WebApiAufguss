using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.Extensions.Options;

namespace WebApi.Services
{
    public class EmailService
    {

        private readonly string _apiKey;

        public EmailService(IConfiguration config)
        {
            // Read from appsettings.json or environment variable
            _apiKey = config["SendGridSettings:ApiKey"] ?? Environment.GetEnvironmentVariable("SendGridSettings");
        }

        public async Task SendTestEmailAsync(string toEmail)
        {
            var options = new SendGridClientOptions
            {
                ApiKey = _apiKey
            };
            options.SetDataResidency("eu");

            var client = new SendGridClient(options);

            //var from = new EmailAddress("david.rantanen@telia.com", "Aufguss"); // replace with your actual from address
            //var to = new EmailAddress(toEmail);
            //var subject = "Test Email from Aufguss";
            //var plainTextContent = "This is a test email from your Razor component.";
            //var htmlContent = "<strong>This is a test email from your Razor component.</strong>";
            //var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            //var response = await client.SendEmailAsync(msg);

            var from = new EmailAddress("david.rantanen@telia.com", "David");
            var subject = "Sending with SendGrid is Fun";
            var to = new EmailAddress("david.rantanen@telia.com", "David");
            var plainTextContent = "and easy to do anywhere, even with C#";
            var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
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
