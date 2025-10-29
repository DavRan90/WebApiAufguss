namespace WebApi.Services.Email
{
    public class EmailSettings
    {
        public string Host { get; set; } = string.Empty;      // e.g. "mail.simply.com"
        public int Port { get; set; } = 587;                 // Usually 587 for STARTTLS
        public string Username { get; set; } = string.Empty; // Your email address
        public string Password { get; set; } = string.Empty; // The password for the mailbox
        public string FromName { get; set; } = "Freja Bad & Gym"; // Optional display name
        public string FromEmail { get; set; } = string.Empty; // Optional, can match Username
    }
}
