using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventTemplatesController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public EventTemplatesController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public IActionResult GetTemplates()
        {
            var templates = context.EventTemplates.OrderBy(e => e.CreatedAt).ToList();
            return Ok(templates);
        }

        [HttpGet("{id}")]
        public IActionResult GetTemplate(int id)
        {
            var evt = context.EventTemplates.Find(id);
            if (evt == null)
            {
                return NotFound();
            }

            return Ok(evt);
        }

        [HttpPost]
        public IActionResult CreateTemplate(EventDto eventTemplateDto)
        {
            var evt = new EventTemplate
            {
                Title = eventTemplateDto.Title,
                Description = eventTemplateDto.Description,
                Start = eventTemplateDto.Start,
                End = eventTemplateDto.End,
                AllDay = eventTemplateDto.AllDay,
                CreatedAt = DateTime.Now,
                MaxSlots = eventTemplateDto.MaxSlots,
                Recurring = eventTemplateDto.Recurring,
                Gender = eventTemplateDto.Gender,
                Hidden = eventTemplateDto.Hidden,
                AllowFriendBooking = eventTemplateDto.AllowFriendBooking,
                IncludeSurvey = eventTemplateDto.IncludeSurvey,
            };

            context.EventTemplates.Add(evt);
            context.SaveChanges();

            return Ok(evt);
        }

        [HttpPut("{id}")]
        public IActionResult EditTemplate(int id, EventDto eventTemplateDto)
        {
            var evt = context.EventTemplates.Find(id);
            if (evt == null)
            {
                return NotFound();
            }

            evt.Title = eventTemplateDto.Title;
            evt.Description = eventTemplateDto.Description;
            evt.Start = eventTemplateDto.Start;
            evt.End = eventTemplateDto.End;
            evt.AllDay = eventTemplateDto.AllDay;
            evt.MaxSlots = eventTemplateDto.MaxSlots;
            evt.Recurring = eventTemplateDto.Recurring;
            evt.Gender = eventTemplateDto.Gender;
            evt.Hidden = eventTemplateDto.Hidden;
            evt.AllowFriendBooking = eventTemplateDto.AllowFriendBooking;
            evt.IncludeSurvey = eventTemplateDto.IncludeSurvey;

            context.SaveChanges();

            return Ok(evt);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTemplate(int id)
        {
            var evt = context.EventTemplates.Find(id);
            if (evt == null)
            {
                return NotFound();
            }

            context.EventTemplates.Remove(evt);
            context.SaveChanges();

            return Ok();
        }
    }
}
