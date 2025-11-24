using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Aufguss.Services.Interface;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Aufguss.Services
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Components.Authorization;
    using Blazored.LocalStorage;
    using System.Text.Json;

    namespace Aufguss.Services
    {
        public class CustomAuthStateProvider : AuthenticationStateProvider, IAuthService
        {
            private readonly ILocalStorageService _storage;
            private readonly IRefreshService _refreshService;
            private const string AccessTokenKey = "token";
            private const string RefreshTokenKey = "refreshToken";

            public CustomAuthStateProvider(ILocalStorageService storage, IRefreshService refreshService)
            {
                _storage = storage;
                _refreshService = refreshService;
            }

            /// <summary>
            /// Main entry point for Blazor to get the current AuthenticationState.
            /// Attempts refresh when token is expired. Never throws; falls back to anonymous user.
            /// </summary>
            public override async Task<AuthenticationState> GetAuthenticationStateAsync()
            {
                try
                {
                    var token = await _storage.GetItemAsync<string>(AccessTokenKey);

                    if (string.IsNullOrWhiteSpace(token))
                        return AnonymousState();

                    var handler = new JwtSecurityTokenHandler();

                    JwtSecurityToken jwt;
                    try
                    {
                        jwt = handler.ReadJwtToken(token);
                    }
                    catch
                    {
                        // Token not readable -> treat as anonymous
                        await ClearTokensAsync();
                        return AnonymousState();
                    }

                    // If token is about to expire or already expired, attempt refresh
                    // Add some clock-skew safety (e.g. 1 minute)
                    var expiry = jwt.ValidTo;
                    if (expiry <= DateTime.UtcNow.AddMinutes(1))
                    {
                        var refreshed = await _refreshService.TryRefreshAsync();
                        if (!refreshed)
                        {
                            await ClearTokensAsync();
                            return AnonymousState();
                        }

                        token = await _storage.GetItemAsync<string>(AccessTokenKey);
                        if (string.IsNullOrWhiteSpace(token))
                            return AnonymousState();

                        try
                        {
                            jwt = handler.ReadJwtToken(token);
                        }
                        catch
                        {
                            await ClearTokensAsync();
                            return AnonymousState();
                        }
                    }

                    var identity = new ClaimsIdentity(ParseClaimsFromJwt(jwt), "jwt");
                    var user = new ClaimsPrincipal(identity);
                    return new AuthenticationState(user);
                }
                catch
                {
                    // Any failure -> anonymous
                    return AnonymousState();
                }
            }

            /// <summary>
            /// Return the raw token (useful for HTTP handlers).
            /// </summary>
            public async Task<string?> GetTokenAsync()
            {
                return await _storage.GetItemAsync<string>(AccessTokenKey);
            }

            /// <summary>
            /// Call after successful login to persist tokens and notify auth state.
            /// Provide the JWT token string and (optionally) refresh token.
            /// </summary>
            public async Task MarkUserAsAuthenticatedAsync(string accessToken, string? refreshToken = null)
            {
                if (!string.IsNullOrWhiteSpace(accessToken))
                    await _storage.SetItemAsync(AccessTokenKey, accessToken);

                if (!string.IsNullOrWhiteSpace(refreshToken))
                    await _storage.SetItemAsync(RefreshTokenKey, refreshToken);

                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwt;
                try
                {
                    jwt = handler.ReadJwtToken(accessToken);
                }
                catch
                {
                    // invalid token - treat as anonymous
                    NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState()));
                    return;
                }

                var identity = new ClaimsIdentity(ParseClaimsFromJwt(jwt), "jwt");
                var user = new ClaimsPrincipal(identity);
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
            }

            /// <summary>
            /// Call on logout. Removes tokens and notifies the UI.
            /// </summary>
            public async Task MarkUserAsLoggedOutAsync()
            {
                await ClearTokensAsync();
                NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState()));
            }

            private static AuthenticationState AnonymousState() =>
                new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            private async Task ClearTokensAsync()
            {
                await _storage.RemoveItemAsync(AccessTokenKey);
                await _storage.RemoveItemAsync(RefreshTokenKey);
            }

            private IEnumerable<Claim> ParseClaimsFromJwt(JwtSecurityToken token)
            {
                // token.Claims is already IEnumerable<Claim>, but sometimes "roles" are in an array or in "role"/"roles"
                var claims = new List<Claim>(token.Claims);

                // Normalize 'roles' if present as JSON array in a "roles" claim (non-standard)
                var roleClaim = token.Claims.FirstOrDefault(c => c.Type == "roles" || c.Type == "role");
                if (roleClaim != null && roleClaim.Value?.StartsWith("[") == true)
                {
                    try
                    {
                        var roles = JsonSerializer.Deserialize<string[]>(roleClaim.Value);
                        if (roles != null)
                        {
                            // remove the original claim and add each role separately
                            claims.RemoveAll(c => c.Type == roleClaim.Type && c.Value == roleClaim.Value);
                            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
                        }
                    }
                    catch
                    {
                        // ignore parse errors; leave original claim
                    }
                }

                return claims;
            }
        }
    }

}
