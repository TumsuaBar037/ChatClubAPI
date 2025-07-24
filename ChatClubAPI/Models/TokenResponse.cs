namespace ChatClubAPI.Models
{
    public class TokenResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? AccessTokenExpires { get; set; }
        public DateTime? RefreshTokenExpires { get; set; }
    }
}
