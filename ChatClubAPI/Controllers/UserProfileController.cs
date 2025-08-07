using ChatClubAPI.Data;
using ChatClubAPI.Models;
using ChatClubAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<UserProfile>> GetUserProfile(Guid accountId)
        {
            var userprofile = await _dbService.GetUserProfile(accountId);
            if (userprofile is null)
            {
                return NotFound("Account not found.");
            }

            return Ok(userprofile);
        }

        [HttpPut("UpdateUserProfile/{id}")]
        public async Task<IActionResult> UpdateUserProfile(int id, UserProfile uiserProfile)
        {
            var result = await _dbService.UpdateUserProfile(id, uiserProfile);

            return result switch
            {
                UpdateResult.NotFound => NotFound(),
                UpdateResult.BadRequest => BadRequest(),
                UpdateResult.Success => NoContent(),
                _ => StatusCode(500, "Unexpected error")
            };
        }
    }
}
