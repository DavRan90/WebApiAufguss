namespace Aufguss
{
    public class Date
    {
        public DateTime CurrentMonthStartDate { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        public int FirstDayOfMonthWeekday { get; set; } = (int)DateTime.Now.AddDays(-1).DayOfWeek;

        public int FirstOfTheMonthWeekdayInteger { get; set; } = (int)new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).DayOfWeek;

        public int PreviousMonthsDay { get; set; } = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddDays((int)new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).DayOfWeek - 1).Day;
        public int CurrentDay { get; set; } = 1;
        public int NextMonth { get; set; } = 1;
    }
}
