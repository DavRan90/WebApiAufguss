using System.ComponentModel.DataAnnotations;

namespace Aufguss.Models
{
    public class UserWithRolesDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        [Required(ErrorMessage = "Förnamn krävs.")]
        [RegularExpression("^[a-zA-ZåäöÅÄÖ]*$", ErrorMessage = "Förnamn får bara innehålla bokstäver.")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Efternamn krävs.")]
        [RegularExpression("^[a-zA-ZåäöÅÄÖ]*$", ErrorMessage = "Efternamn får bara innehålla bokstäver.")]
        public string SurName { get; set; }
        [RegularExpression(@"^\+?\d{7,15}$", ErrorMessage = "Ange ett giltigt telefonnummer med 7 till 15 siffror.")]
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new();
        public Gender? Gender { get; set; }
        public string? AufgussFriendId { get; set; }
    }
}
