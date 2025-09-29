namespace Aufguss
{
    public class BookingDto
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Tel { get; set; } = "";
        public DateTime BookedAt { get; set; }
        public bool Reserve { get; set; } = false;
        public DateTime? ReserveAt { get; set; }
        public DateTime? UnbookedAt { get; set; }
        public bool? Unbooked { get; set; } = false;
        public string? UserId { get; set; }
        public int EventId { get; set; }
        public string? CancellationToken { get; set; }
    }
}
