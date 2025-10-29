using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IAuditService audit;

        public EventsController(ApplicationDbContext context, IAuditService audit)
        {
            this.context = context;
            this.audit = audit;
        }

        // Hämta endast synliga event (för gäster)
        [HttpGet]
        public IActionResult GetVisibleEvents()
        {
            var settings = context.SiteSettings.FirstOrDefault();
            if (settings == null)
                return BadRequest("Inställningar saknas.");

            var events = context.Events
                .Where(e => !e.Hidden || (e.Hidden && e.Start.Date.AddDays(-settings.AufgussHiddenDaysInAdvance) <= DateTime.UtcNow))
                .OrderBy(e => e.Start)
                .ToList();

            // Justera Hidden-fältet dynamiskt
            var adjustedEvents = events.Select(e =>
            {
                if (e.Hidden && e.Start.Date.AddDays(-settings.AufgussHiddenDaysInAdvance) <= DateTime.UtcNow)
                {
                    return new Event
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Start = e.Start,
                        End = e.End,
                        Gender = e.Gender,
                        MaxSlots = e.MaxSlots,
                        Description = e.Description,
                        AllowFriendBooking = e.AllowFriendBooking,
                        IncludeSurvey = e.IncludeSurvey,
                        Hidden = false
                    };
                }
                return e;
            }).ToList();

            return Ok(adjustedEvents);
        }

        // Hämta alla event (för admin)
        [HttpGet("admin")]
        public IActionResult GetEvents()
        {
            var events = context.Events.OrderBy(e => e.Start).ToList();
            return Ok(events);
        }

        // Hämta ett specifikt event
        [HttpGet("{id}")]
        public IActionResult GetEvent(int id)
        {
            var settings = context.SiteSettings.FirstOrDefault();
            if (settings == null)
                return BadRequest("Inställningar saknas.");

            var evt = context.Events.Find(id);
            if (evt == null)
                return NotFound();

            if (evt.Hidden && evt.Start.Date.AddDays(-settings.AufgussHiddenDaysInAdvance) <= DateTime.UtcNow)
            {
                return Ok(new Event
                {
                    Id = evt.Id,
                    Title = evt.Title,
                    Start = evt.Start,
                    End = evt.End,
                    Gender = evt.Gender,
                    MaxSlots = evt.MaxSlots,
                    Description = evt.Description,
                    AllowFriendBooking = evt.AllowFriendBooking,
                    IncludeSurvey = evt.IncludeSurvey,
                    Hidden = false
                });
            }

            return Ok(evt);
        }

        // Skapa nytt event
        [HttpPost]
        public async Task<IActionResult> CreateEvent(EventDto eventDto)
        {
            var evt = new Event
            {
                Title = eventDto.Title,
                Description = eventDto.Description,
                Start = eventDto.Start,
                End = eventDto.End,
                AllDay = eventDto.AllDay,
                CreatedAt = DateTime.Now,
                MaxSlots = eventDto.MaxSlots,
                Recurring = eventDto.Recurring,
                Gender = eventDto.Gender,
                Hidden = eventDto.Hidden,
                AllowFriendBooking = eventDto.AllowFriendBooking,
                IncludeSurvey = eventDto.IncludeSurvey
            };

            context.Events.Add(evt);
            await context.SaveChangesAsync();

            await audit.LogEventAsync(evt.Id, "Skapat", null, $"Event '{evt.Title}' skapades.");

            return Ok(evt);
        }

        // Ändra event
        [HttpPut("{id}")]
        public async Task<IActionResult> EditEvent(int id, EventDto eventDto)
        {
            var evt = await context.Events.FindAsync(id);
            if (evt == null)
                return NotFound();

            if (eventDto.Hidden && context.Bookings.Any(b => b.EventId == id))
                return BadRequest(new { message = "Bokningen går inte att dölja eftersom det finns gäster bokade." });

            if (eventDto.Gender != evt.Gender && context.Bookings.Any(b => b.EventId == id))
                return BadRequest(new { message = "Det går inte att ändra kön för passet efter att bokningar har skett." });

            var activeBookings = context.Bookings.Count(b => b.EventId == evt.Id && !b.Unbooked && !b.Reserve);
            if (eventDto.MaxSlots < activeBookings)
                return BadRequest(new { message = "Det finns fler anmälda än platser." });

            evt.Title = eventDto.Title;
            evt.Description = eventDto.Description;
            evt.Start = eventDto.Start;
            evt.End = eventDto.End;
            evt.AllDay = eventDto.AllDay;
            evt.MaxSlots = eventDto.MaxSlots;
            evt.Recurring = eventDto.Recurring;
            evt.Gender = eventDto.Gender;
            evt.Hidden = eventDto.Hidden;
            evt.AllowFriendBooking = eventDto.AllowFriendBooking;
            evt.IncludeSurvey = eventDto.IncludeSurvey;

            await context.SaveChangesAsync();
            await audit.LogEventAsync(evt.Id, "Ändrat", null, $"Event '{evt.Title}' ändrades.");

            return Ok(evt);
        }

        // Ta bort event
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var evt = await context.Events.FindAsync(id);
            if (evt == null)
                return NotFound();

            var bookings = context.Bookings.Where(b => b.EventId == id).ToList();
            context.Bookings.RemoveRange(bookings);
            context.Events.Remove(evt);

            await context.SaveChangesAsync();
            await audit.LogEventAsync(evt.Id, "Borttaget", null, $"Event '{evt.Title}' togs bort (inkl. {bookings.Count} bokningar).");

            return Ok();
        }

        // Hämta senaste händelser (för admin)
        [HttpGet("admin/logs")]
        public IActionResult GetEventLogs()
        {
            var logs = context.EventAudits
                .OrderByDescending(a => a.Timestamp)
                .Take(200)
                .ToList();

            return Ok(logs);
        }
    }
}
