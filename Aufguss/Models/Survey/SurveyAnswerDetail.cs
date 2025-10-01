namespace Aufguss.Models.Survey
{
    public class SurveyAnswerDetail
    {
        public int Id { get; set; }
        public int AnswerId { get; set; }
        public int QuestionId { get; set; }
        public double Answer { get; set; }
        public SurveyAnswer SurveyAnswer { get; set; } = default!;
    }
}
