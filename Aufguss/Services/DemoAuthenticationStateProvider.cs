using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Aufguss.Services.Interface;
using Microsoft.AspNetCore.Components.Authorization;

namespace Aufguss.Services
{
    public class DemoAuthenticationStateProvider : AuthenticationStateProvider, IAuthService
    {
        // A fixed, pre-generated demo JWT (can be any valid structure)
        private const string DemoJwtToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9." +
            "eyJzdWIiOiJkZW1vLXVzZXItaWQiLCJlbWFpbCI6ImRhdmlkLnJhbnRhbmVuQHRlbGlhLmNvbSIsInBob25lX251bWJlciI6Iis0NjcwMTIzNDU2NyIsImZ1bGxOYW1lIjoiRGVtbyBVc2VyIiwiZmlyc3ROYW1lIjoiRGVtbyIsImdlbmRlciI6Ik1hbGUiLCJmcmllbmRJZCI6ImRlbW8tZnJpZW5kLWlkIiwianRpIjoiZGVtby1qdGktMTIzNCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwiZXhwIjoyMDAwMDAwMDAwfQ." +
            "dummySignatureForDemoOnly1234567890";

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, "demo-user-id"),
                new Claim(JwtRegisteredClaimNames.Email, "david.rantanen@telia.com"),
                new Claim(JwtRegisteredClaimNames.PhoneNumber, "+46701234567"),
                new Claim("fullName", "Demo User"),
                new Claim("firstName", "Demo"),
                new Claim("gender", "Male"),
                new Claim("friendId", "demo-friend-id"),
                new Claim(JwtRegisteredClaimNames.Jti, "demo-jti-1234"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "DemoAuth");

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }

        public Task<string?> GetTokenAsync()
        {
            // Just return a fixed mock JWT for demo mode
            return Task.FromResult<string?>(DemoJwtToken);
        }
    }
}
