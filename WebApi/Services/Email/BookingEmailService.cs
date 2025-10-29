namespace WebApi.Services.Email
{
    public class BookingEmailService
    {
        private readonly ResendEmailService _resendEmailService;

        public BookingEmailService(ResendEmailService resendEmailService)
        {
            _resendEmailService = resendEmailService;
        }

        public async Task SendBookingConfirmationAsync(string customerEmail, string customerName, string bookingDetails)
        {
            var subject = "Booking Confirmation";
            var htmlContent = $@"
            <p>Hello {customerName},</p>
            <p>Thank you for your booking!</p>
            <p>Details: {bookingDetails}</p>";

            await _resendEmailService.SendEmailAsync(customerEmail, subject, htmlContent);
        }

        // Similarly, you can add SendBookingCancellationAsync, SendReminderAsync, etc.
    }

}
