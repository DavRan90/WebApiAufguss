using Aufguss.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Aufguss.Services.Interface
{
    public interface IAuthService
    {
        Task<AuthenticationState?> GetAuthenticationStateAsync();
    }
}
