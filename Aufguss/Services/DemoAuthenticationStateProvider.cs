using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Aufguss.Services.Interface;
using Microsoft.AspNetCore.Components.Authorization;

namespace Aufguss.Services
{
    public class DemoAuthenticationStateProvider : AuthenticationStateProvider, IAuthService
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "demo-user-id"),
            new Claim(JwtRegisteredClaimNames.Email, "demo@demo.com"),
            new Claim(JwtRegisteredClaimNames.PhoneNumber, "+46701234567"),
            new Claim("fullName", "Demo User"),
            new Claim("firstName", "Demo"),
            new Claim("gender", "Male"), // or "Female", based on your test case
            new Claim("friendId", "demo-friend-id"), // Optional: Only add if testing friend booking
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "Admin") // or "User", "Guest", etc.
        }, "DemoAuth");

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
    }
}
