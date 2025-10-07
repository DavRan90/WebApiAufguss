namespace Aufguss.Models
{
    public class ApplicationUser
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public List<string> Roles { get; set; } = new();

        public string? AufgussFriendId { get; set; }
    }
}

