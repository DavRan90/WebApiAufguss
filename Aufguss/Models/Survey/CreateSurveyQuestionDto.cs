namespace Aufguss.Models.Survey
{
    public class CreateSurveyQuestionDto
    {
        public int SurveyId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Order { get; set; }

        public string EmojiMin { get; set; } = "😐";
        public string EmojiMax { get; set; } = "😍";
    }
}
