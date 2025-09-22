using System.ComponentModel.DataAnnotations;

namespace Aufguss
{
    public class EntryDto
    {
        [Required(ErrorMessage = "Titel är obligatorisk")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Beskrivning är obligatorisk")]
        public string Description { get; set; } = "";
        public string Image { get; set; } = "";
        public string Url { get; set; } = "";
        public string UrlTitle { get; set; } = "";
    }
}
