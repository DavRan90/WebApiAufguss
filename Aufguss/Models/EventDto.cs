using Aufguss.Models;
using System.ComponentModel.DataAnnotations;

namespace Aufguss
{
    public class EventDto
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Start { get; set; } = DateTime.Now;
        public DateTime? End { get; set; } = DateTime.Now;
        public bool AllDay { get; set; }
        public bool Hidden { get; set; }
        public bool Recurring { get; set; }
        public int MaxSlots { get; set; }
        public Gender Gender { get; set; }
        public bool AllowFriendBooking { get; set; }
        public bool IncludeSurvey { get; set; }
    }
}
