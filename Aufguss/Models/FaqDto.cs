using System.ComponentModel.DataAnnotations;

namespace Aufguss.Models
{
    public class FaqDto
    {
        [Required(ErrorMessage = "Fråga är obligatorisk")]
        public string Question { get; set; } = "";

        [Required(ErrorMessage = "Svar är obligatorisk")]
        public string Answer { get; set; } = "";
        public bool ShowQuestion { get; set; } = true;
        public int Position { get; set; }
    }
}
