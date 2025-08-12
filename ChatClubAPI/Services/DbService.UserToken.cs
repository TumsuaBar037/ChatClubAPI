using ChatClubAPI.Data;
using ChatClubAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatClubAPI.Services
{
    public partial class DbService
    {
        public async Task<UserToken?> GetUserToken(Guid accountId)
        {
            return await _context.UserTokens.FirstOrDefaultAsync(x => x.AccountId == accountId);
        }

        public async Task<UpdateResult> UpdateToken(int id, UserToken userToken)
        {
            if (id != userToken.Id)
            {
                return UpdateResult.BadRequest;
            }

            _context.Entry(userToken).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                var exists = await _context.UserTokens.AnyAsync(e => e.Id == id);
                if (!exists)
                    return UpdateResult.NotFound;

                throw;
            }

            return UpdateResult.Success;
        }
    }
}
