using System.ComponentModel.DataAnnotations;

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
    }
}
