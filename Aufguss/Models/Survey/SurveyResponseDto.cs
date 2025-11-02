namespace Aufguss.Models.Survey
{
    public class SurveyResponseDto
    {
        public string EventId { get; set; } = string.Empty;
        public int Temperature { get; set; }
        public int Scent { get; set; }
        public int Duration { get; set; }
    }
}
