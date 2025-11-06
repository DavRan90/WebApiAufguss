namespace WebApi.Models
{
    public class AuthDto
    {
        public class RefreshRequest
        {
            public string RefreshToken { get; set; }
        }

        public class LogoutRequest
        {
            public string RefreshToken { get; set; }
        }

        public class LoginResponseDto
        {
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public DateTime AccessTokenExpires { get; set; }
            public DateTime RefreshTokenExpires { get; set; }
        }

    }
}
