namespace Aufguss
{
    public class Entry
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Image { get; set; } = "";
        public string Url { get; set; } = "";
        public string UrlTitle { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
