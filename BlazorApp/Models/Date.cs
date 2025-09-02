namespace BlazorApp.Models
{
    public class Date
    {
        public DateTime DateNow { get; set; } = DateTime.Now;
        public DateTime StartOfMonth { get; set; } = DateTime.Now.AddDays(-1);
        public int FirstDayOfMonth { get; set; } = (int)DateTime.Now.AddDays(-1).DayOfWeek; // Gör om med fasta värden på år och dag, flexibelt på månad
        public int CurrentDay { get; set; } = 1;
        public int NextMonth { get; set; } = 1;
    }
}
