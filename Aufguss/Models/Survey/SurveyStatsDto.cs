namespace Aufguss.Models.Survey
{
    public class SurveyStatsDto
    {
        public string EventId { get; set; } = string.Empty;
        public double AvgTemperature { get; set; }
        public double AvgScent { get; set; }
        public double AvgDuration { get; set; }
        public int Count { get; set; }
    }
}
