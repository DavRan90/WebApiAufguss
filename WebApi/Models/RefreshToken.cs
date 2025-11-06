namespace WebApi.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }          // Consider storing a hashed token instead of plaintext
        public string UserId { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Expires { get; set; }
        public DateTime? Revoked { get; set; }    // when it was revoked
        public bool IsRevoked => Revoked != null;
        public bool IsExpired => DateTime.UtcNow >= Expires;
    }

}
