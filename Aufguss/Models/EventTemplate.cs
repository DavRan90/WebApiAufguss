namespace Aufguss.Models
{
    public class EventTemplate
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public bool Hidden { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Recurring { get; set; }
        public bool CreateNews { get; set; }
        public int MaxSlots { get; set; }
        public Gender Gender { get; set; }
        public bool AllowFriendBooking { get; set; }
        public bool IncludeSurvey { get; set; }
    }
}
