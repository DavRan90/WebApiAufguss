using Aufguss.Models;
using System.ComponentModel.DataAnnotations;
using static Aufguss.Pages.Events.Create;

namespace Aufguss
{
    public class EventDto
    {
        [Required(ErrorMessage = "Titel är obligatoriskt")]
        public string Title { get; set; } = "";
        [Required(ErrorMessage = "Beskrivning är obligatoriskt")]
        public string Description { get; set; } = "";
        [FutureDate]
        public DateTime Start { get; set; } = DateTime.Now;
        [EndAfterStart]
        public DateTime? End { get; set; } = DateTime.Now;
        public bool AllDay { get; set; }
        public bool Hidden { get; set; }
        public bool Recurring { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Max antal gäster måste vara större än 0.")]
        public int MaxSlots { get; set; }
        public Gender Gender { get; set; }
        public bool AllowFriendBooking { get; set; }
        public bool IncludeSurvey { get; set; }
    }
}
