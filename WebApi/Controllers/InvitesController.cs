using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvitesController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public InvitesController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("invitees")]
        public async Task<IActionResult> GetInvitees([FromQuery] string inviteeId)
        {
            var invites = await _context.Invites
        .Where(i => i.InviteeId == inviteeId)
        .ToListAsync();

            if (invites == null || invites.Count == 0)
            {
                return NotFound("No invites found");
            }

            var userIds = invites
                .Select(i => i.InviterId)
                .Distinct()
                .ToList();

            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.SurName}");

            var result = invites.Select(i => new InviteDto
            {
                Id = i.Id,
                InviterId = i.InviterId,
                InviterName = users.TryGetValue(i.InviterId, out var name) ? name : "(unknown)",
                InviteeId = i.InviteeId,
                InviteeName = "(current user)", // Optional, if you want to show their own name
                InvitedAt = i.InvitedAt,
                AcceptedAt = i.AcceptedAt,
                Accepted = i.Accepted,
                Invited = i.Invited
            }).ToList();

            return Ok(result);
        }

        [HttpGet("inviters")]
        public async Task<IActionResult> GetInviters([FromQuery] string inviterId)
        {
            var invites = await _context.Invites
        .Where(i => i.InviterId == inviterId)
        .ToListAsync();

            if (invites == null || invites.Count == 0)
            {
                return NotFound("No invites found");
            }

            var userIds = invites
                .Select(i => i.InviteeId)
                .Distinct()
                .ToList();

            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.SurName}");

            var result = invites.Select(i => new InviteDto
            {
                Id = i.Id,
                InviterId = i.InviterId,
                InviterName = "(current user)", // optional
                InviteeId = i.InviteeId,
                InviteeName = users.TryGetValue(i.InviteeId, out var name) ? name : "(unknown)",
                InvitedAt = i.InvitedAt,
                AcceptedAt = i.AcceptedAt,
                Accepted = i.Accepted,
                Invited = i.Invited
            }).ToList();

            return Ok(result);
        }

        [HttpPost]
        public IActionResult SendInvite(InviteDto inviteDto)
        {
            var invite = new Invite
            {
                Invited = true,
                InviterId = inviteDto.InviterId,
                InviteeId = inviteDto.InviteeId,
                InvitedAt = DateTime.UtcNow,
                Accepted = false,
            };

            _context.Invites.Add(invite);
            _context.SaveChanges();

            return Ok(invite);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> InviteAccepted(int id, [FromBody] InviteDto model)
        {
            var invite = await _context.Invites.Where(i => i.Id == id).SingleOrDefaultAsync();
            if (invite == null) return NotFound();

            
            invite.Accepted = model.Accepted;
            invite.AcceptedAt = DateTime.UtcNow;
            invite.Invited = model.Invited;

            var inviter = await _context.Users.FirstOrDefaultAsync(u => u.Id == invite.InviterId);
            var invitee = await _context.Users.FirstOrDefaultAsync(u => u.Id == invite.InviteeId);

            if (inviter == null || invitee == null)
            {
                return BadRequest("One or both users not found.");
            }

            inviter.AufgussFriendId = invitee.Id;
            invitee.AufgussFriendId = inviter.Id;

            await _context.SaveChangesAsync();

            return Ok("Invite accepted and users linked successfully.");
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvite(int id)
        {
            var invite = await _context.Invites.Where(i => i.Id == id).SingleOrDefaultAsync();
            if (invite == null) return NotFound();

            var inviter = await _context.Users.FirstOrDefaultAsync(u => u.Id == invite.InviterId);
            var invitee = await _context.Users.FirstOrDefaultAsync(u => u.Id == invite.InviteeId);

            if (inviter == null || invitee == null)
            {
                return BadRequest("One or both users not found.");
            }

            inviter.AufgussFriendId = null;
            invitee.AufgussFriendId = null;

            _context.Invites.Remove(invite);
            await _context.SaveChangesAsync();

            return Ok("Invite removed");
        }
    }
}
