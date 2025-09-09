namespace Aufguss.Models
{
    public class Faq
    {
        public int Id { get; set; }
        public string Question { get; set; } = "";
        public string Answer { get; set; } = "";
        public bool ShowQuestion { get; set; } = true;
        public int Position { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
