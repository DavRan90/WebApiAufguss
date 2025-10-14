using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public EventsController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public IActionResult GetVisibleEvents()
        {
            var settings = context.SiteSettings.FirstOrDefault();
            //var today = DateTime.UtcNow.Date;
            //var visibleFromDate = today;
            //var visibleToDate = today.AddDays(settings.AufgussHiddenDaysInAdvance);

            var events = context.Events
                .Where(e => !e.Hidden || (e.Hidden && e.Start.Date.AddDays(-settings.AufgussHiddenDaysInAdvance) <= DateTime.UtcNow))
                .OrderBy(e => e.Start)
                .ToList();

            // Adjust Hidden property on the fly for events that pass the threshold
            var adjustedEvents = events.Select(e =>
            {
                if (e.Hidden && e.Start.Date.AddDays(-settings.AufgussHiddenDaysInAdvance) <= DateTime.UtcNow)
                {
                    // Return a copy or an anonymous object with Hidden set to false
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
                        Hidden = false,
                        // Copy other properties as needed
                    };
                }
                else
                {
                    return e;
                }
            }).ToList();

            return Ok(adjustedEvents);
        }

        [HttpGet("admin")]
        public IActionResult GetEvents()
        {
            var events = context.Events.OrderBy(e => e.Start).ToList();
            return Ok(events);
        }

        //[HttpGet]
        //public IActionResult GetVisibleEvents()
        //{
        //    var settings = context.SiteSettings.FirstOrDefault();
        //    var today = DateTime.UtcNow.Date;
        //    var visibleFromDate = today.AddDays(-settings.AufgussHiddenDaysInAdvance);

        //    var events = context.Events
        //        .Where(e => !e.Hidden || (e.Hidden && e.Start.Date >= visibleFromDate))
        //        .OrderBy(e => e.Start)
        //        .ToList();

        //    return Ok(events);
        //}

        [HttpGet("{id}")]
        public IActionResult GetEvent(int id)
        {
            var settings = context.SiteSettings.FirstOrDefault();

            var evt = context.Events.Find(id);
            if (evt == null)
            {
                return NotFound();
            }
            if (evt.Hidden && evt.Start.Date.AddDays(-settings.AufgussHiddenDaysInAdvance) <= DateTime.UtcNow)
            {
                // Return a copy or an anonymous object with Hidden set to false
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
                    Hidden = false,
                });
            }
            else
            {
                return Ok(evt);
            }
        }

        [HttpPost]
        public IActionResult CreateEvent(EventDto eventDto)
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
                IncludeSurvey = eventDto.IncludeSurvey,
            };

            context.Events.Add(evt);
            context.SaveChanges();

            return Ok(evt);
        }

        [HttpPut("{id}")]
        public IActionResult EditEvent(int id, EventDto eventDto)
        {
            var evt = context.Events.Find(id);
            if (evt == null)
            {
                return NotFound();
            }

            if (eventDto.Hidden == true &&
                context.Bookings.Any(b => b.EventId == id))
            {
                return BadRequest(new { message = "Bokningen går inte att dölja eftersom det finns gäster bokade." });
            }

            if (eventDto.Gender != evt.Gender &&
                context.Bookings.Any(b => b.EventId == id))
            {
                return BadRequest(new { message = "Det går inte att ändra kön för passet efter bokning har skett" });
            }

            if (eventDto.MaxSlots < context.Bookings.Where(b => b.EventId == evt.Id && b.Unbooked == false && b.Reserve == false).Count())
            {
                return BadRequest(new { message = "Det finns fler anmälda än platser" });
            }

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

            context.SaveChanges();

            return Ok(evt);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteEvent(int id)
        {
            var evt = context.Events.Find(id);
            if (evt == null)
            {
                return NotFound();
            }
            // Also remove all associated bookings
            var bookings = context.Bookings.Where(b => b.EventId == id).ToList();
            foreach (var booking in bookings)
            {
                context.Bookings.Remove(booking);
            }

            context.Events.Remove(evt);
            context.SaveChanges();

            return Ok();
        }
    }
}
