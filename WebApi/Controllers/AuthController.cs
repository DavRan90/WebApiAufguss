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
//using Microsoft.AspNetCore.Identity.Data;
using SendGrid.Helpers.Mail;
using WebApi.Services;
using static WebApi.Models.AuthDto;
using System.Security.Cryptography;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AuthController(UserManager<ApplicationUser> userManager,
                      RoleManager<IdentityRole> roleManager,
                      IConfiguration config, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
            _context = context;
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
            Console.WriteLine("LOGIN START");
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                Console.WriteLine("FIND USER: " + (user != null));

                if (user == null)
                {
                    Console.WriteLine("USER NOT FOUND");
                    return Unauthorized("Invalid credentials.");
                }

                var passwordOk = await _userManager.CheckPasswordAsync(user, model.Password);
                Console.WriteLine("PASSWORD OK: " + passwordOk);

                if (!passwordOk)
                    return Unauthorized("Invalid credentials.");

                Console.WriteLine("GENERATING JWT...");
                string accessToken;
                try
                {
                    accessToken = await GenerateJwtToken(user);
                    Console.WriteLine("JWT GENERATED");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR during GenerateJwtToken: " + ex);
                    return StatusCode(500, new { message = "Token generation failed", details = ex.Message });
                }

                Console.WriteLine("CREATING REFRESH TOKEN...");
                var refreshToken = GenerateRefreshToken(user.Id, daysValid: 14);

                try
                {
                    _context.RefreshTokens.Add(refreshToken);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("REFRESH TOKEN SAVED, ID: " + refreshToken.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR saving refresh token: " + ex);
                    return StatusCode(500, new { message = "Failed to save refresh token", details = ex.Message });
                }

                var response = new LoginResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token,
                    AccessTokenExpires = DateTime.UtcNow.AddHours(1),
                    RefreshTokenExpires = refreshToken.Expires
                };

                Console.WriteLine("LOGIN COMPLETE - returning response");
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("UNEXPECTED ERROR in Login: " + ex);
                return StatusCode(500, new { message = "Unexpected error", details = ex.Message });
            }
        }



        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            // validate config
            var jwtKey = _config["Jwt:Key"];
            var jwtIssuer = _config["Jwt:Issuer"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT key is missing from configuration (Jwt:Key)");
            if (string.IsNullOrEmpty(jwtIssuer))
                throw new InvalidOperationException("JWT issuer is missing from configuration (Jwt:Issuer)");

            var userRoles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new Claim(JwtRegisteredClaimNames.PhoneNumber, user.PhoneNumber ?? ""),
        new Claim("fullName", $"{user.FirstName} {user.SurName}".Trim()),
        new Claim("firstName", user.FirstName ?? ""),
        new Claim("gender", user.Gender?.ToString() ?? ""),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            if (!string.IsNullOrEmpty(user.AufgussFriendId))
                claims.Add(new Claim("friendId", user.AufgussFriendId));

            foreach (var role in userRoles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            if (string.IsNullOrEmpty(request?.RefreshToken))
                return BadRequest("Refresh token required.");

            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken);

            if (refreshToken == null)
                return NotFound("Refresh token not found.");

            refreshToken.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok("Logged out successfully.");
        }



        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
                return BadRequest("No refresh token provided.");

            var existing = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken);

            if (existing == null || existing.IsRevoked || existing.IsExpired)
                return Unauthorized("Invalid or expired refresh token.");

            var user = await _userManager.FindByIdAsync(existing.UserId);
            if (user == null)
                return Unauthorized("Invalid token user.");

            // Revoke old token
            existing.Revoked = DateTime.UtcNow;

            // Create new refresh token (rotation)
            var newRefreshToken = GenerateRefreshToken(user.Id, daysValid: 14);
            _context.RefreshTokens.Add(newRefreshToken);

            await _context.SaveChangesAsync();

            var newJwt = await GenerateJwtToken(user);

            var response = new LoginResponseDto
            {
                AccessToken = newJwt,
                RefreshToken = newRefreshToken.Token,
                AccessTokenExpires = DateTime.UtcNow.AddHours(1),
                RefreshTokenExpires = newRefreshToken.Expires
            };

            return Ok(response);
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
        private RefreshToken GenerateRefreshToken(string userId, int daysValid = 7)
        {
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString("N") + Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
                UserId = userId,
                Created = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(daysValid)
            };

            return refreshToken;
        }

    }
}