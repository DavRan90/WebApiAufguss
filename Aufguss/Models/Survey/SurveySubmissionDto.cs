namespace Aufguss.Models.Survey
{
    public class SurveySubmissionDto
    {
        public int SurveyId { get; set; }           // The ID of the survey being answered
        public string? UserId { get; set; }
        public List<SurveyAnswerDto> Answers { get; set; } = new();
    }
}
