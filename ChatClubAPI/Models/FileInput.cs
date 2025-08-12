namespace ChatClubAPI.Models
{
    public class FileInput
    {
        public IFormFile? UserProfile { get; set; }
        public string? Name { get; set; }
        public int Gender { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
