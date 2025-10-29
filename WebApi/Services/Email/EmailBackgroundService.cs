using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Hosting;

namespace WebApi.Services.Email

{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IBackgroundEmailQueue _emailQueue;
        private readonly IServiceProvider _serviceProvider;

        public EmailBackgroundService(IBackgroundEmailQueue emailQueue, IServiceProvider serviceProvider)
        {
            _emailQueue = emailQueue;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Wait for the next email to process
                var emailMessage = await _emailQueue.DequeueAsync(stoppingToken);
                Console.WriteLine($"📬 Sending email to {emailMessage.To}");
                try
                {
                    // Create a new DI scope for scoped services like ResendEmailService
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var emailSender = scope.ServiceProvider.GetRequiredService<ResendEmailService>();
                        await emailSender.SendEmailAsync(
                            emailMessage.To,
                            emailMessage.Subject,
                            emailMessage.Body
                        );
                    }
                }
                catch (Exception ex)
                {
                    // Log exception but don't stop the loop
                    Console.WriteLine($"Failed to send email to {emailMessage.To}: {ex.Message}");
                }
            }
        }
    }
}