using ChatClubAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatClubAPI.Services
{
    public partial class DbService
    {
        public async Task<bool> CreateUserLocation(UserLocation userLocation)
        {
            try
            {
                await _context.UserLocations.AddAsync(userLocation);
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
