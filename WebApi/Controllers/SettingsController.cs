using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class SettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/settings
        [HttpGet]
        public async Task<ActionResult<SiteSettings>> GetSettings()
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();

            if (settings == null)
                return NotFound("Settings not found.");

            return Ok(settings);
        }

        // PUT: api/settings
        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] SiteSettings updatedSettings)
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();

            if (settings == null)
                return NotFound("Settings not found.");

            // Update values
            settings.MaxBookingsAufguss = updatedSettings.MaxBookingsAufguss;
            settings.AufgussHiddenDaysInAdvance = updatedSettings.AufgussHiddenDaysInAdvance;
            settings.MaxBookingsVattenfys = updatedSettings.MaxBookingsVattenfys;
            settings.VattenfysHiddenDaysInAdvance = updatedSettings.VattenfysHiddenDaysInAdvance;
            settings.MaxNews = updatedSettings.MaxNews;

            await _context.SaveChangesAsync();

            return Ok("Settings updated successfully.");
        }
    }
}
