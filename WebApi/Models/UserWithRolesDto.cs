namespace WebApi.Models
{
    public class UserWithRolesDto
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; }
        public Gender? Gender { get; set; }

        public string? AufgussFriendId { get; set; }
        public string? AufgussFriendName { get; set; }
    }
}
