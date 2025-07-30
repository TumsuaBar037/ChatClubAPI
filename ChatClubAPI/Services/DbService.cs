using ChatClubAPI.Data;

namespace ChatClubAPI.Services
{
    public partial class DbService
    {
        private readonly ClubChatContext _context;

        public DbService(ClubChatContext context)
        {
            _context = context;
        }
    }
}
