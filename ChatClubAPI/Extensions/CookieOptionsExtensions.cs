namespace ChatClubAPI.Extensions
{
    public static class CookieOptionsExtensions
    {
        public static CookieOptions DefaultOptions(int days = 7, bool secure = true)
        {
            return new CookieOptions
            {
                HttpOnly = false,
                Secure = secure,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(days),
                Path = "/"
            };
        }
    }
}
