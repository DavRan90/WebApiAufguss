namespace Aufguss.Models
{
    public class FaqDto
    {
        public string Question { get; set; } = "";
        public string Answer { get; set; } = "";
        public bool ShowQuestion { get; set; }
        public int Position { get; set; }
    }
}
