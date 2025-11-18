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

        //[HttpGet]
        //public async Task<IActionResult> GetAllInvites()
        //{
        //    var invites = await _context.Users
        //        .Select(i => new UserDto
        //        {
        //            Id = i.Id,
        //            Email = i.Email,
        //            FirstName = i.FirstName,
        //            SurName = i.SurName,
        //            PhoneNumber = i.PhoneNumber,
        //            Gender = i.Gender,
        //            AufgussFriendId = i.AufgussFriendId
        //        })
        //        .ToListAsync();

        //    return Ok(invites);
        //}

        [HttpGet]
        public async Task<IActionResult> GetAllInvites()
        {
            var invites = await _context.Invites.ToListAsync();

            return Ok(invites);
        }

        [HttpGet("invitees")]
        public async Task<IActionResult> GetInvitees([FromQuery] string inviteeId)
        {
            var invites = await _context.Invites
        .Where(i => i.InviteeId == inviteeId)
        .ToListAsync();

            //if (invites == null || invites.Count == 0)
            //{
            //    return NotFound("No invites found");
            //}

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

            //if (invites == null || invites.Count == 0)
            //{
            //    return NotFound("No invites found");
            //}

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
        public async Task<IActionResult> SendInvite(InviteDto inviteDto)
        {
            // 🔍 Fetch both users
            var inviter = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == inviteDto.InviterId);
            var invitee = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == inviteDto.InviteeId);

            if (inviter == null || invitee == null)
                return BadRequest(new { message = "One or both users not found." });

            if (inviter.Gender != invitee.Gender)
                return BadRequest(new { message = "Du kan bara bjuda in användare av samma kön." });

            // 🔁 Optional: prevent duplicates
            var existingInvite = await _context.Invites
                .FirstOrDefaultAsync(i =>
                    i.InviterId == inviteDto.InviterId &&
                    i.InviteeId == inviteDto.InviteeId &&
                    !i.Accepted);

            if (existingInvite != null)
                return BadRequest(new { message = "En inbjudan har redan skickats till denna användare." });

            var invite = new Invite
            {
                Invited = true,
                InviterId = inviteDto.InviterId,
                InviteeId = inviteDto.InviteeId,
                InvitedAt = DateTime.UtcNow,
                Accepted = false,
            };

            _context.Invites.Add(invite);
            await _context.SaveChangesAsync();

            return Ok(invite);
        }

        [HttpPost("send-by-phone")]
        public async Task<IActionResult> SendInviteByPhone([FromBody] SendInviteByPhoneRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                return BadRequest(new { message = "Phone number is required." });

            // Normalize Swedish phone numbers (could also use your existing normalization helper)
            var phone = request.PhoneNumber.Trim().Replace(" ", "");
            if (phone.StartsWith("0"))
                phone = "+46" + phone.Substring(1);
            if (phone.StartsWith("00"))
                phone = "+" + phone.Substring(2);

            // Get current logged in user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Unauthorized();

            // Find invitee by phone number
            var invitee = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
            if (invitee == null)
            {
                return NotFound(new { message = "Ingen användare hittades med detta telefonnummer." });
            }

            if (currentUser.Gender != invitee.Gender)
                return BadRequest(new { message = "Du kan bara bjuda in användare av samma kön." });

            // Prevent duplicate pending invites
            var existingInvite = await _context.Invites
                .FirstOrDefaultAsync(i =>
                    i.InviterId == currentUser.Id &&
                    i.InviteeId == invitee.Id &&
                    !i.Accepted);

            if (existingInvite != null)
                return BadRequest(new { message = "En inbjudan har redan skickats till denna användare." });

            // Create invite
            var invite = new Invite
            {
                InviterId = currentUser.Id,
                InviteeId = invitee.Id,
                InvitedAt = DateTime.UtcNow,
                Accepted = false,
                Invited = true
            };

            _context.Invites.Add(invite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Invite sent successfully." });
        }

        public class SendInviteByPhoneRequest
        {
            public string PhoneNumber { get; set; }
        }


        [HttpPut("{id}")]
        //public async Task<IActionResult> AcceptInvite(int id, [FromBody] InviteDto dto)
        public async Task<IActionResult> AcceptInvite(int id)
        {
            var invite = await _context.Invites.FirstOrDefaultAsync(i => i.Id == id);
            if (invite == null)
                return NotFound("Invite not found.");

            // Accept the invite
            invite.Accepted = true;
            invite.AcceptedAt = DateTime.UtcNow;
            invite.Invited = false;

            var inviter = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == invite.InviterId);
            var invitee = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == invite.InviteeId);

            if (inviter == null || invitee == null)
                return BadRequest("One or both users not found.");

            // 🔥 Handle existing friendship (if any)
            if (!string.IsNullOrEmpty(invitee.AufgussFriendId))
            {
                var oldFriend = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Id == invitee.AufgussFriendId);

                if (oldFriend != null)
                {
                    // Remove existing friendship on both ends
                    oldFriend.AufgussFriendId = null;
                    invitee.AufgussFriendId = null;
                }

                // Optionally, remove invites between them
                var oldInvites = await _context.Invites
                    .Where(i =>
                        (i.InviterId == invitee.Id && i.InviteeId == oldFriend.Id) ||
                        (i.InviterId == oldFriend.Id && i.InviteeId == invitee.Id))
                    .ToListAsync();

                _context.Invites.RemoveRange(oldInvites);
            }

            // 🔥 Optional: Handle inviter's old friend too (only if one-friend rule applies both ways)
            if (!string.IsNullOrEmpty(inviter.AufgussFriendId))
            {
                var inviterOldFriend = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Id == inviter.AufgussFriendId);

                if (inviterOldFriend != null)
                {
                    inviterOldFriend.AufgussFriendId = null;
                    inviter.AufgussFriendId = null;

                    var oldInvites = await _context.Invites
                        .Where(i =>
                            (i.InviterId == inviter.Id && i.InviteeId == inviterOldFriend.Id) ||
                            (i.InviterId == inviterOldFriend.Id && i.InviteeId == inviter.Id))
                        .ToListAsync();

                    _context.Invites.RemoveRange(oldInvites);
                }
            }

            // 🔁 Now set new mutual friendship
            inviter.AufgussFriendId = invite.InviteeId;
            invitee.AufgussFriendId = invite.InviterId;

            inviter.AufgussFriendName = $"{invitee.FirstName} {invitee.SurName}";
            invitee.AufgussFriendName = $"{inviter.FirstName} {inviter.SurName}";


            // 🔥 Remove all other invites involving either user
            var otherInvites = await _context.Invites
                .Where(i => i.Id != invite.Id &&
                    (i.InviteeId == invite.InviteeId || i.InviterId == invite.InviteeId ||
                     i.InviteeId == invite.InviterId || i.InviterId == invite.InviterId))
                .ToListAsync();

            _context.Invites.RemoveRange(otherInvites);

            await _context.SaveChangesAsync();

            return Ok("Friendship accepted and updated. Old connections removed.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvite(int id)
        {
            var invite = await _context.Invites.SingleOrDefaultAsync(i => i.Id == id);
            if (invite == null)
                return NotFound("Invite not found");

            var inviter = await _context.Users.FirstOrDefaultAsync(u => u.Id == invite.InviterId);
            var invitee = await _context.Users.FirstOrDefaultAsync(u => u.Id == invite.InviteeId);

            if (inviter == null || invitee == null)
                return BadRequest("One or both users not found.");

            // Clear the AufgussFriendId references
            if (inviter.AufgussFriendId == invitee.Id)
                inviter.AufgussFriendId = null;

            if (invitee.AufgussFriendId == inviter.Id)
                invitee.AufgussFriendId = null;

            _context.Invites.Remove(invite);

            await _context.SaveChangesAsync();

            return Ok("Invite removed and friendship broken");
        }
    }
}
