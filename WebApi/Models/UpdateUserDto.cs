namespace WebApi.Models
{
    public class UpdateUserDto
    {
        public string FirstName { get; set; }
        public string SurName { get; set; }
        public string PhoneNumber { get; set; }
        public Gender Gender { get; set; }
    }
}
