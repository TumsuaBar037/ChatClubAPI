namespace ChatClubAPI.Models
{
    public class SendLogin
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int LocationId { get; set; }
        public string? LocationName { get; set; }
        public string? Latitude { get; set; }
        public string? Longtitude { get; set; }
    }
}
