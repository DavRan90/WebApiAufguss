using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Models
{
    public class EventDto
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Start { get; set; } = DateTime.Now;
        public DateTime? End { get; set; }
        public bool AllDay { get; set; }
        public bool Recurring { get; set; }
        public int MaxSlots { get; set; }
        public Gender Gender { get; set; }
    }
}
