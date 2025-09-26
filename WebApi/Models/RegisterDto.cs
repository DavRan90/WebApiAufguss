using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class RegisterDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public Gender? Gender { get; set; }
    }
}
