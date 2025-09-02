using System.ComponentModel.DataAnnotations;

namespace BlazorAufguss.Models
{
    public class EventDto
    {
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Start { get; set; } = DateTime.Now;
        public DateTime? End { get; set; }
        public bool AllDay { get; set; }
    }
}
