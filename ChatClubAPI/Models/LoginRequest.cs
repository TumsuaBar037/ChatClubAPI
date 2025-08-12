namespace ChatClubAPI.Models
{
    public class LoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
