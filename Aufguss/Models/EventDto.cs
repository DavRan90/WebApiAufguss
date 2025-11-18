using Aufguss.Models;
using System.ComponentModel.DataAnnotations;

namespace Aufguss
{
    public class EventDto
    {
        [Required(ErrorMessage = "Titel är obligatoriskt")]
        public string Title { get; set; } = "";
        [Required(ErrorMessage = "Beskrivning är obligatoriskt")]
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
