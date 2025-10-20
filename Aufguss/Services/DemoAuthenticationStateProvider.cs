using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace Aufguss.Services
{
    public class DemoAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity(new[]
            {
            new Claim("sub", "demo-user-id"),
            new Claim("firstName", "Demo"),
            new Claim(ClaimTypes.Surname, "User"),
            new Claim(ClaimTypes.Email, "demo@demo.com"),
            new Claim(ClaimTypes.MobilePhone, "+46701234567"),
            new Claim(ClaimTypes.Role, "Admin")
        }, "DemoAuth");

            var user = new ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
    }
}
