namespace Aufguss.Models
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;    // The JWT
        public string RefreshToken { get; set; } = string.Empty;   // The refresh token
        public DateTime AccessTokenExpires { get; set; }           // Optional: JWT expiry
        public DateTime RefreshTokenExpires { get; set; }          // Optional: refresh token expiry
    }
}
