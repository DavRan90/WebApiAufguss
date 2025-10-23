using Aufguss.Models;

namespace Aufguss.Services.Interface
{
    public interface IUserService
    {
        Task<List<UserWithRolesDto>> GetUsersAsync();
        Task<UserWithRolesDto?> GetUserAsync(string? id, string? phoneNumber);
        Task EditUserAsync(string id, UserWithRolesDto userDto);
        Task RemoveUserAsync(string id);
    }
}
