using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly MailgunEmailService _emailService;

        public EmailController(MailgunEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send-test")]
        public async Task<IActionResult> SendTestEmail()
        {
            var success = await _emailService.SendEmailAsync(
                "david.rantanen@telia.com",
                "Hello from Mailgun",
                "Congratulations David Rantanen, you just sent an email with Mailgun!"
            );

            if (!success)
                return StatusCode(500, "Failed to send email");

            return Ok("Email sent!");
        }
    }
}
