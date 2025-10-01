namespace Aufguss.Models.Survey
{
    public class SurveyQuestion
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }

        public string Text { get; set; } = string.Empty;

        public int Order { get; set; }

        // Emoji for slider scale
        public string? EmojiMin { get; set; }
        public string? EmojiMax { get; set; }

        public Survey Survey { get; set; } = default!;
    }
}
