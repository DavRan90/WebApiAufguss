using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly ResendEmailService _resendEmailService;

        public EmailController(ResendEmailService resendEmailService)
        {
            _resendEmailService = resendEmailService;
        }

        [HttpPost("test-email")]
        public async Task<IActionResult> TestEmail()
        {
            try
            {
                var to = "david.rantanen@telia.com";
                var subject = "Test email from API";
                var htmlContent = "<p>Hello from Resend test!</p>";

                var response = await _resendEmailService.SendTestEmailAsync(to, subject, htmlContent);

                // Return the full response from Resend for debugging
                return Ok(new { message = $"Email request sent via Resend to {to}!", resendResponse = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error sending email", error = ex.Message });
            }
        }
    }
}
