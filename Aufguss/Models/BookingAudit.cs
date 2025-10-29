namespace Aufguss.Models
{
    public class BookingAudit
    {
        public int Id { get; set; }               // Primary key
        public int BookingId { get; set; }        // Related booking
        public string Action { get; set; }        // "Booked", "Unbooked", "Promoted", etc.
        public string UserEmail { get; set; }     // Who performed the action
        public DateTime Timestamp { get; set; }   // When it happened
        public string? Details { get; set; }      // Optional extra info
    }
}
