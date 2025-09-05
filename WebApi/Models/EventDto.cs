using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace WebApi.Models
{
    public class EventDto
    {
        [Required]
        public string Title { get; set; } = "";

        [MaxLength(500)]
        public string Description { get; set; } = "";

        [Required]
        public DateTime Start { get; set; }
        public DateTime? End { get; set; }
        public bool AllDay { get; set; }
        public bool Hidden { get; set; }
        public bool Recurring { get; set; }
        public int MaxSlots { get; set; }
        public Gender Gender { get; set; }
    }
}
