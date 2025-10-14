namespace Aufguss.Models
{
    public class EventTemplateDto
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public bool AllDay { get; set; }
        public bool Hidden { get; set; }
        public bool Recurring { get; set; }
        public int MaxSlots { get; set; }
        public Gender Gender { get; set; }
        public bool AllowFriendBooking { get; set; }
        public bool IncludeSurvey { get; set; }
    }
}
