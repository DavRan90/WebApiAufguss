using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi.Services.Email
{
    public class BackgroundEmailQueue : IBackgroundEmailQueue
    {
        private readonly ConcurrentQueue<EmailMessage> _emails = new();
        private readonly SemaphoreSlim _signal = new(0);

        public void QueueEmail(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(to)) return;

            _emails.Enqueue(new EmailMessage
            {
                To = to,
                Subject = subject,
                Body = body
            });

            _signal.Release();
        }

        public async Task<EmailMessage> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _emails.TryDequeue(out var message);
            return message!;
        }
    }
}
