using ChatClubAPI.Data;
using ChatClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatClubAPI.Services
{
    public partial class DbService
    {
        public async Task<UserProfile?> GetUserProfile(Guid accountId)
        {
            return await _context.UserProfiles.FirstOrDefaultAsync(x => x.AccountId == accountId);
        }

        public async Task<UpdateResult> UpdateUserProfile(int id, UserProfile userProfile)
        {
            if (id != userProfile.Id)
            {
                return UpdateResult.BadRequest;
            }

            _context.Entry(userProfile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.UserProfiles.AnyAsync(e => e.Id == id);
                if (!exists)
                    return UpdateResult.NotFound;

                throw;
            }

            return UpdateResult.Success;
        }
    }
}
