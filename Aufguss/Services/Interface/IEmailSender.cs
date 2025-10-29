namespace Aufguss.Services.Interface
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string htmlContent);
    }
}
