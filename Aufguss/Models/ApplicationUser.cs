using Microsoft.AspNet.Identity.EntityFramework;

namespace Aufguss.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}

