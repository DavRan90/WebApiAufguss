using Aufguss.Models;

namespace Aufguss.Services.Interface
{
    public interface IUserService
    {
        Task<List<UserWithRolesDto>> GetUsersAsync();
        Task<UserWithRolesDto?> GetUserByIdAsync(string id);
    }
}
