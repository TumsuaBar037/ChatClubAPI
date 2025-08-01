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

        public async Task<Account?> GetAccount(Guid id)
        {
            return await _context.Accounts.FindAsync(id);
        }
    }
}
