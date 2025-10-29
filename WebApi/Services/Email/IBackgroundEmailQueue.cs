namespace WebApi.Services.Email
{
    public interface IBackgroundEmailQueue
    {
        void QueueEmail(string to, string subject, string body);
        Task<EmailMessage> DequeueAsync(CancellationToken cancellationToken);
    }
}
