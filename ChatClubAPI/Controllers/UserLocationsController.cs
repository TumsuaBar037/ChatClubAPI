using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatClubAPI.Data;

namespace ChatClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserLocationsController : ControllerBase
    {
        private readonly ClubChatContext _context;

        public UserLocationsController(ClubChatContext context)
        {
            _context = context;
        }

        // GET: api/UserLocations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserLocation>>> GetUserLocations()
        {
            return await _context.UserLocations.ToListAsync();
        }

        // GET: api/UserLocations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserLocation>> GetUserLocation(long id)
        {
            var userLocation = await _context.UserLocations.FindAsync(id);

            if (userLocation == null)
            {
                return NotFound();
            }

            return userLocation;
        }

        // PUT: api/UserLocations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserLocation(long id, UserLocation userLocation)
        {
            if (id != userLocation.Id)
            {
                return BadRequest();
            }

            _context.Entry(userLocation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserLocationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/UserLocations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserLocation>> PostUserLocation(UserLocation userLocation)
        {
            _context.UserLocations.Add(userLocation);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserLocation", new { id = userLocation.Id }, userLocation);
        }

        // DELETE: api/UserLocations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserLocation(long id)
        {
            var userLocation = await _context.UserLocations.FindAsync(id);
            if (userLocation == null)
            {
                return NotFound();
            }

            _context.UserLocations.Remove(userLocation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserLocationExists(long id)
        {
            return _context.UserLocations.Any(e => e.Id == id);
        }
    }
}
