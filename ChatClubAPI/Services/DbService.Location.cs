using ChatClubAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatClubAPI.Services
{
    public partial class DbService
    {
        public async Task<List<Location>> GetLocation()
        {
            return await _context.Locations.AsNoTracking().ToListAsync();
        }

        public async Task<Location?> GetLocation(int id)
        {
            return await _context.Locations.FindAsync(id);
        }
    }
}
