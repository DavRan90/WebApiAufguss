using Microsoft.AspNetCore.Identity;

namespace WebApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string SurName { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
