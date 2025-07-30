using ChatClubAPI.Data;

namespace ChatClubAPI.Services
{
    public partial class DbService
    {
        public async Task<bool> CreateAccount(Account account)
        {
            try
            {
                await _context.Accounts.AddAsync(account);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
