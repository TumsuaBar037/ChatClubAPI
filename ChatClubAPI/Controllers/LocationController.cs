using ChatClubAPI.Data;
using ChatClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationController : ControllerBase
    {
        private readonly DbService _dbService;

        public LocationController(DbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("GetLocation/{id}")]
        public async Task<IActionResult> GetLocation(int id)
        {
            return Ok(await _dbService.GetLocation(id));
        }
    }
}
