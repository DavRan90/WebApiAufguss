using Aufguss.Models;
using Aufguss.Services.Interface;

namespace Aufguss.Services.Demo
{
    public class DemoUserService : IUserService
    {
        private readonly List<UserWithRolesDto> demoUsers = new()
    {
        new UserWithRolesDto
    {
        Id = "demo-user-id",
        FirstName = "Demo",
        SurName = "User",
        Email = "demo@demo.com",
        PhoneNumber = "+46701234567",
        Gender = Gender.Male,
        AufgussFriendId = "friend-user-id"
    },
    new UserWithRolesDto
    {
        Id = "friend-user-id",
        FirstName = "Friend",
        SurName = "Person",
        Email = "friend@example.com",
        PhoneNumber = "+46709876543"
    }
        // Add more mock data as needed
    };

        public Task<List<UserWithRolesDto>> GetUsersAsync()
        {
            return Task.FromResult(demoUsers);
        }

        public Task<UserWithRolesDto?> GetUserByIdAsync(string id)
        {
            var ev = demoUsers.FirstOrDefault(e => e.Id == "demo-user-id");
            return Task.FromResult(ev);
        }
        
    }
}
