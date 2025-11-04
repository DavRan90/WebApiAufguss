using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using WebApi.Models;
using WebApi.Services;
using System.Security.Claims;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly FriendService _friendService;

        public UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, FriendService friendService)
        {
            _userManager = userManager;
            _context = context;
            _friendService = friendService;
        }

        // GET: api/users
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.ToList();

            var usersWithRoles = new List<UserWithRolesDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserWithRolesDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    SurName = user.SurName,
                    PhoneNumber = user.PhoneNumber,
                    AufgussFriendId = user.AufgussFriendId,
                    Roles = roles.ToList(),
                    Gender = user.Gender
                });
            }

            return Ok(usersWithRoles);
        }
        // GET: api/users/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            //var userId = User.FindFirst("sub")?.Value;
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userManager.Users
    .Include(u => u.AufgussFriend)
    .SingleOrDefaultAsync(u => u.Id == userId);

            var dto = new UserWithRolesDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                SurName = user.SurName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                AufgussFriendId = user.AufgussFriendId,
                AufgussFriendName = user.AufgussFriend != null
        ? $"{user.AufgussFriend.FirstName} {user.AufgussFriend.SurName}"
        : null
            };
            return Ok(dto);
        }


        // PUT: api/users/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto model)
        {
            var currentUserId = User.FindFirst("sub")?.Value;
            var isAdmin = User.IsInRole("Admin");

            if (currentUserId != id && !isAdmin)
                return Forbid();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.SurName = model.SurName;
            user.PhoneNumber = NormalizeSwedishPhone(model.PhoneNumber);
            user.Gender = model.Gender;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User updated successfully");
        }

        // DELETE: api/users/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return NotFound("User not found");

                var invites = await _context.Invites
                    .Where(i => i.InviteeId == id || i.InviterId == id)
                    .ToListAsync();

                foreach (var invite in invites)
                {
                    var success = await _friendService.RemoveInviteAndBreakFriendship(invite.Id);
                    if (!success)
                        return BadRequest($"Failed to remove invite ID {invite.Id}");
                }

                

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);


                await _context.SaveChangesAsync();
                return Ok("User and related invites deleted successfully");
            }
            catch (Exception ex)
            {
                // This will send back a JSON error so you can see it
                return StatusCode(500, new { message = "Unexpected error", details = ex.Message });
            }
        }

        // Reuse the normalization method from your AuthController
        private static string NormalizeSwedishPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;

            phone = System.Text.RegularExpressions.Regex.Replace(phone, @"[\s\-\(\)]", "");

            if (phone.StartsWith("00")) phone = "+" + phone[2..];
            else if (phone.StartsWith("0")) phone = "+46" + phone[1..];

            return phone;
        }
    }
}
