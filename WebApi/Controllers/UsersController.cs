using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;
using WebApi.Services;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly FriendService _friendService;

    public UsersController(UserManager<ApplicationUser> userManager, FriendService friendService)
    {
        _userManager = userManager;
        _friendService = friendService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userManager.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                SurName = u.SurName,
                PhoneNumber = u.PhoneNumber,
                Gender = u.Gender,
                AufgussFriendId = u.AufgussFriendId
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.Users
            .Include(u => u.AufgussFriend)
            .SingleOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound(new { error = "User not found." });

        var roles = await _userManager.GetRolesAsync(user);

        var dto = new UserWithRolesDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            SurName = user.SurName,
            PhoneNumber = user.PhoneNumber,
            Roles = roles.ToList(),
            AufgussFriendId = user.AufgussFriendId,
            AufgussFriendName = user.AufgussFriend != null
                ? $"{user.AufgussFriend.FirstName} {user.AufgussFriend.SurName}"
                : null
        };

        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto model)
    {
        var currentUserId = User.FindFirst("sub")?.Value;
        var isAdmin = User.IsInRole("Admin");

        if (currentUserId != id && !isAdmin)
            return Forbid();

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound();

        user.FirstName = model.FirstName;
        user.SurName = model.SurName;
        user.PhoneNumber = PhoneHelper.NormalizeSwedishPhone(model.PhoneNumber);
        user.Gender = model.Gender;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "User updated successfully." });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { error = "User not found." });

        // Remove all friend and invite relationships
        var success = await _friendService.RemoveAllFriendRelationsForUserAsync(id);
        if (!success)
            return BadRequest(new { error = "Failed to clean up friendships." });

        // Delete the user
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "User and related invites deleted successfully." });
    }

}
