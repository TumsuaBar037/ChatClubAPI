using ChatClubAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly DbService _dbService;

        public UserProfileController(DbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("GetUserProfile/{accountId}")]
        public async Task<IActionResult> GetAccount(Guid accountId)
        {
            var userprofile = await _dbService.GetUserProfile(accountId);
            if (userprofile is null)
            {
                return NotFound("Account not found.");
            }

            return Ok(userprofile);
        }
    }
}
