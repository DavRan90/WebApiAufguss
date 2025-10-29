using Aufguss.Services.Interface;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Aufguss.Services.API
{
    public class ApiEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public ApiEmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlContent)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["EmailSettings:From"]));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = htmlContent };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:Port"]), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["EmailSettings:Username"], _config["EmailSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
