namespace Aufguss.Models.Survey
{
    public class Survey
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<SurveyQuestion> Questions { get; set; } = new List<SurveyQuestion>();
    }
}
