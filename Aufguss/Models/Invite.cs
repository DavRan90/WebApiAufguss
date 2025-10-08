namespace Aufguss.Models
{
    public class Invite
    {
        public int Id { get; set; }
        public string InviterId { get; set; }
        public string InviteeId { get; set; }
        public DateTime InvitedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public bool Accepted { get; set; }
        public bool Invited { get; set; }

    }
}
