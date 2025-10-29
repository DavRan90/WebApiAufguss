namespace WebApi.Models
{
    public class EventAudit
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string Action { get; set; } = string.Empty; // Created / Updated / Deleted
        public string? UserEmail { get; set; } // optional if logged in user
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; }
    }
}
