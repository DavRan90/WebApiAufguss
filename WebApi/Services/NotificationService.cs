using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.Extensions.Options;

namespace WebApi.Services
{
    public class NotificationService
    {
        private readonly string _apiKey;

        public NotificationService(IOptions<SendGridSettings> options)
        {
            _apiKey = options.Value.ApiKey;
        }

        public async Task SendBookingEmailAsync(string toEmail, string toName, string eventTitle)
        {
            Console.WriteLine("SendBookingEmailAsync called");
            //var options = new SendGridClientOptions
            //{
            //    ApiKey = _apiKey
            //};
            //options.SetDataResidency("eu");
            //var client = new SendGridClient(options);
            var client = new SendGridClient(_apiKey);

            var from = new EmailAddress("david.rantanen@telia.com", "Aufguss");
            var to = new EmailAddress(toEmail, toName);
            var subject = "Bokningsbekräftelse";
            var plainTextContent = $"Hej {toName}, din bokning för \"{eventTitle}\" har registrerats.";
            var htmlContent = $"<strong>Hej {toName}</strong><br>Din bokning för <b>{eventTitle}</b> har registrerats.";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);

            Console.WriteLine($"SendGrid Response Status Code: {response.StatusCode}");

            if ((int)response.StatusCode >= 400)
            {
                string responseBody = await response.Body.ReadAsStringAsync();
                Console.WriteLine("SendGrid error response: " + responseBody);
            }
        }
    }
}
