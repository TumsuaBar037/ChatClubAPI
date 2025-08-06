using ChatClubAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatClubAPI.Services
{
    public partial class DbService
    {
        public async Task<List<UserInterest>> GetUserInterest(Guid accountId)
        {
            return await _context.UserInterests.Where(x => x.AccountId == accountId)
                .AsNoTracking().ToListAsync();
        }

        public async Task<(bool Success, string? ErrorMessage, UserInterest? Data)> AddUserInterest(UserInterest userInterest)
        {
            try
            {
                _context.UserInterests.Add(userInterest);
                await _context.SaveChangesAsync();
                return (true, null, userInterest);
            }
            catch (DbUpdateException ex)
            {
                return (false, ex.Message, null);
            }
            catch (Exception ex)
            {
                return (false, "Internal server error: " + ex.Message, null);
            }
        }
    }
}
