using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public AdminController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet("audit-logs")]
        public IActionResult GetAuditLogs()
        {
            var logs = new
            {
                Bookings = context.BookingAudits.OrderByDescending(l => l.Timestamp).Take(100).ToList(),
                Events = context.EventAudits.OrderByDescending(l => l.Timestamp).Take(100).ToList()
            };

            return Ok(logs);
        }
    }
}
