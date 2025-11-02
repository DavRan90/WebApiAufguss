namespace Aufguss.Models.Survey
{
    public class SurveyResponse
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string EventId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int Temperature { get; set; }
        public int Scent { get; set; }
        public int Duration { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
