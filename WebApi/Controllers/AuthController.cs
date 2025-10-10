using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(UserManager<ApplicationUser> userManager,
                      RoleManager<IdentityRole> roleManager,
                      IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        [HttpGet("users")]
        //[Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetUsers()
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

        [HttpGet("user")]
        public async Task<IActionResult> GetUser([FromQuery] string? id, [FromQuery] string? phoneNumber)
        {
            ApplicationUser user = null;

            if (!string.IsNullOrEmpty(id))
            {
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            }
            else if (!string.IsNullOrEmpty(phoneNumber))
            {
                var normalizedPhone = NormalizeSwedishPhone(phoneNumber);
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == normalizedPhone);
            }

            if (user == null)
            {
                return NotFound("No user found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userWithRoles = new UserWithRolesDto()
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                SurName = user.SurName,
                PhoneNumber = user.PhoneNumber,
                AufgussFriendId = user.AufgussFriendId,
                Roles = roles.ToList(),
                Gender = user.Gender,
            };

            return Ok(userWithRoles);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            // 1. Check if user already exists
            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
                return BadRequest("User with this email already exists.");

            var existingPhoneNumber = await _userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == NormalizeSwedishPhone(model.PhoneNumber));
            if (existingPhoneNumber != null)
                return BadRequest("User with this phone number already exists.");

            // 2. Create new user object
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = NormalizeSwedishPhone(model.PhoneNumber),
                FirstName = model.FirstName,
                SurName = model.SurName,
                Gender = model.Gender,
            };

            // 3. Create user with password
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // 4. Ensure "User" role exists (optional but recommended)
            var roleExists = await _roleManager.RoleExistsAsync("User");
            if (!roleExists)
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("User"));
                if (!roleResult.Succeeded)
                    return StatusCode(500, "Failed to create default role.");
            }

            // 5. Assign "User" role
            var assignRoles = await _userManager.AddToRoleAsync(user, "User");
            if (!assignRoles.Succeeded)
                return BadRequest(assignRoles.Errors);

            return Ok("User registered successfully");
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAdmin([FromBody] RegisterDto model)
        {
            // 1. Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return BadRequest("User with this email already exists.");

            // 2. Create new user object
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FirstName = model.FirstName,
                SurName = model.SurName,
            };

            // 3. Create user with password
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest("Registration failed. Please check your input.");

            // 4. Ensure "User" role exists (optional but recommended)
            var roleExists = await _roleManager.RoleExistsAsync("Admin");
            if (!roleExists)
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("Admin"));
                if (!roleResult.Succeeded)
                    return StatusCode(500, "Failed to create default role.");
            }

            // 5. Assign "User" role
            var assignRoles = await _userManager.AddToRoleAsync(user, "Admin");
            //var assignRoles = await _userManager.AddToRolesAsync(user, new[] { "SuperAdmin", "Admin" });
            if (!assignRoles.Succeeded)
                return BadRequest(assignRoles.Errors);

            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = await GenerateJwtToken(user);
                return Ok(new { token });
            }
            return Unauthorized("Invalid credentials.");
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user); // returns List<string>

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new Claim(JwtRegisteredClaimNames.PhoneNumber, user.PhoneNumber ?? ""),
        new Claim("fullName", (user.FirstName + " " + user.SurName) ?? ""),
        new Claim("firstName", user.FirstName ?? ""),
        new Claim("gender", user.Gender.ToString() ?? ""),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            if (!string.IsNullOrEmpty(user.AufgussFriendId))
            {
                claims.Add(new Claim("friendId", user.AufgussFriendId));
            }

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static string NormalizeSwedishPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            // Remove spaces, dashes, parentheses
            phone = Regex.Replace(phone, @"[\s\-\(\)]", "");

            // If it starts with 00 (e.g., 0046), replace with +
            if (phone.StartsWith("00"))
                phone = "+" + phone.Substring(2);

            // If it starts with 0 (e.g., 0701234567), replace with +46
            if (phone.StartsWith("0"))
                phone = "+46" + phone.Substring(1);

            // Ensure it's now in +46... format
            return phone;
        }

    }
}