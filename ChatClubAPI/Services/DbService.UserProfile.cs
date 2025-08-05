using ChatClubAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatClubAPI.Services
{
    public partial class DbService
    {
        public async Task<UserProfile?> GetUserProfile(Guid accountId)
        {
            return await _context.UserProfiles.FirstOrDefaultAsync(x => x.AccountId == accountId);
        }
    }
}
