using Aufguss.Models;
using Aufguss.Services.Interface;

namespace Aufguss.Services.Demo
{
    public class DemoUserService : IUserService
    {
        private readonly List<UserWithRolesDto> _users = new()
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
                Id = "2b4ec2a0-0eaf-4a16-9412-ebd982c8c098",
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
            return Task.FromResult(_users);
        }

        public Task<UserWithRolesDto?> GetUserAsync(string? id, string? phoneNumber)
        {
            UserWithRolesDto? user = null;

            if (!string.IsNullOrEmpty(id))
            {
                user = _users.FirstOrDefault(u => u.Id == id);
            }
            else if (!string.IsNullOrEmpty(phoneNumber))
            {
                var normalizedPhone = NormalizeSwedishPhone(phoneNumber);
                user = _users.FirstOrDefault(u => u.PhoneNumber == normalizedPhone);
            }

            if (user == null)
            {
                throw new HttpRequestException("User not found", null, System.Net.HttpStatusCode.NotFound);
            }

            return Task.FromResult(user);
        }
        public Task EditUserAsync(string id, UserWithRolesDto userDto)
        {
            var existing = _users.FirstOrDefault(u => u.Id == id);
            if (existing == null)
                throw new Exception($"Användare med ID {id} hittades inte.");

            existing.FirstName = userDto.FirstName;
            existing.SurName = userDto.SurName;
            existing.PhoneNumber = NormalizeSwedishPhone(userDto.PhoneNumber);

            return Task.CompletedTask;
        }
        public Task RemoveUserAsync(string id)
        {
            var booking = _users.SingleOrDefault(b => b.Id == id);
            if (booking != null)
            {
                _users.Remove(booking);
            }

            return Task.CompletedTask;
        }

        private static string NormalizeSwedishPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;

            phone = System.Text.RegularExpressions.Regex.Replace(phone, @"[\s\-\(\)]", "");

            if (phone.StartsWith("00")) phone = "+" + phone[2..];
            else if (phone.StartsWith("0")) phone = "+46" + phone[1..];

            return phone;
        }

    }
}
