using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using WebApi.Models;


namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        // =======================
        // === GET ALL USERS ===
        // =======================
        [HttpGet("users")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserWithRolesDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(MapUserToDto(user, roles));
            }

            return Ok(userList);
        }

        // =======================
        // === GET SINGLE USER ===
        // =======================
        [HttpGet("user")]
        [Authorize]
        public async Task<IActionResult> GetUser([FromQuery] string? id, [FromQuery] string? phoneNumber)
        {
            if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(phoneNumber))
                return BadRequest(new { error = "Either 'id' or 'phoneNumber' must be provided." });

            var normalizedPhone = NormalizeSwedishPhone(phoneNumber);
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id || u.PhoneNumber == normalizedPhone);

            if (user == null)
                return NotFound(new { error = "No user found." });

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(MapUserToDto(user, roles));
        }

        // =======================
        // === USER REGISTER ===
        // =======================
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Email and phone uniqueness
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return BadRequest(new { error = "User with this email already exists." });

            var normalizedPhone = NormalizeSwedishPhone(model.PhoneNumber);
            if (await _userManager.Users.AnyAsync(u => u.PhoneNumber == normalizedPhone))
                return BadRequest(new { error = "User with this phone number already exists." });

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = normalizedPhone,
                FirstName = model.FirstName,
                SurName = model.SurName,
                Gender = model.Gender
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            await EnsureRoleExistsAsync("User");
            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { message = "User registered successfully." });
        }

        // =======================
        // === CREATE ADMIN ===
        // =======================
        [HttpPost("create")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateAdmin([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return BadRequest(new { error = "User with this email already exists." });

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = NormalizeSwedishPhone(model.PhoneNumber),
                FirstName = model.FirstName,
                SurName = model.SurName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            await EnsureRoleExistsAsync("Admin");
            await _userManager.AddToRoleAsync(user, "Admin");

            return Ok(new { message = "Admin created successfully." });
        }

        // =======================
        // === LOGIN ===
        // =======================
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { error = "Invalid email or password." });

            var token = await GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);
            var userDto = MapUserToDto(user, roles);

            return Ok(new
            {
                token,
                user = userDto
            });
        }

        // =======================
        // === HELPERS ===
        // =======================
        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim("phone_number", user.PhoneNumber ?? ""),
                new Claim("fullName", $"{user.FirstName} {user.SurName}".Trim()),
                new Claim("firstName", user.FirstName ?? ""),
                new Claim("gender", user.Gender?.ToString() ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrEmpty(user.AufgussFriendId))
                claims.Add(new Claim("friendId", user.AufgussFriendId));

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key missing")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:TokenExpiryMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UserWithRolesDto MapUserToDto(ApplicationUser user, IList<string> roles) =>
            new()
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
            };

        private async Task EnsureRoleExistsAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        public static string NormalizeSwedishPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return null;

            phone = Regex.Replace(phone, @"[\s\-\(\)]", "");
            if (phone.StartsWith("00")) phone = "+" + phone[2..];
            else if (phone.StartsWith("0")) phone = "+46" + phone[1..];

            return phone;
        }
    }
}
