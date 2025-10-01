namespace Aufguss.Models.Survey
{
    public class SurveyAnswer
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string? RespondentId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public ICollection<SurveyAnswerDetail> Details { get; set; } = new List<SurveyAnswerDetail>();
    }
}
