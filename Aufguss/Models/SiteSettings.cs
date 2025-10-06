namespace Aufguss.Models
{
    public class SiteSettings
    {
        public int Id { get; set; }
        public int MaxBookingsAufguss { get; set; }
        public int AufgussHiddenDaysInAdvance { get; set; }
        public int MaxBookingsVattenfys { get; set; }
        public int VattenfysHiddenDaysInAdvance { get; set; }
        public int MaxNews { get; set; }
    }
}
