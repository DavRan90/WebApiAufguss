using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntriesController : ControllerBase
    {
        private readonly ApplicationDbContext context;

        public EntriesController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public IActionResult GetEntries()
        {
            var entries = context.Entries.OrderByDescending(e => e.Id).ToList();
            return Ok(entries);
        }

        [HttpGet("{id}")]
        public IActionResult GetEntry(int id)
        {
            var entry = context.Entries.Find(id);
            if (entry == null)
            {
                return NotFound();
            }

            return Ok(entry);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult CreateEntry(EntryDto entryDto)
        {
            var entry = new Entry
            {
                Title = entryDto.Title,
                Description = entryDto.Description,
                Image = entryDto.Image,
                Url = entryDto.Url,
                CreatedAt = DateTime.Now,
            };

            context.Entries.Add(entry);
            context.SaveChanges();

            return Ok(entry);
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult EditEntry(int id, EntryDto entryDto)
        {
            var entry = context.Entries.Find(id);
            if (entry == null)
            {
                return NotFound();
            }

            entry.Title = entryDto.Title;
            entry.Description = entryDto.Description;
            entry.Image = entryDto.Image;
            entry.Url = entryDto.Url;

            context.SaveChanges();

            return Ok(entry);
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteEntry(int id)
        {
            var entry = context.Entries.Find(id);
            if (entry == null)
            {
                return NotFound();
            }

            context.Entries.Remove(entry);
            context.SaveChanges();

            return Ok();
        }
    }
}
