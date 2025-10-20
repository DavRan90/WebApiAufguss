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
                UserName = "demo@demo.com",
                FirstName = "Demo",
                SurName = "User",
                Email = "demo@demo.com",
                PhoneNumber = "+46701234567",
                Gender = Gender.Male,
                AufgussFriendId = "friend-user-id",
                Roles = new List<string> { "Admin" }
            },
            new UserWithRolesDto
            {
                Id = "friend-user-id",
                UserName = "friend@example.com",
                FirstName = "Friend",
                SurName = "Person",
                Email = "friend@example.com",
                PhoneNumber = "+46709876543",
                Gender = Gender.Female,
                Roles = new List<string> { "User" }
            },
            new UserWithRolesDto
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "anna@demo.com",
                FirstName = "Anna",
                SurName = "Svensson",
                Email = "anna@demo.com",
                PhoneNumber = "+46701111111",
                Gender = Gender.Female,
                AufgussFriendId = "friend-user-id",
                Roles = new List<string> { "User" }
            },
            new UserWithRolesDto
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "bjorn@demo.com",
                FirstName = "Björn",
                SurName = "Johansson",
                Email = "bjorn@demo.com",
                PhoneNumber = "+46702222222",
                Gender = Gender.Male,
                Roles = new List<string> { "User", "Admin" }
            },
            new UserWithRolesDto
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "carina@demo.com",
                FirstName = "Carina",
                SurName = "Nilsson",
                Email = "carina@demo.com",
                PhoneNumber = "+46703333333",
                Gender = Gender.Female,
                Roles = new List<string> { "User" }
            }
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
