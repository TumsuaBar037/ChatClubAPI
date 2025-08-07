using ChatClubAPI.Data;
using ChatClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace ChatClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserInterestController : ControllerBase
    {
        private readonly DbService _dbService;

        public UserInterestController(DbService dbService)
        {
            _dbService = dbService;
        }

        [HttpGet("GetUserInterest/{accountId}")]
        public async Task<IEnumerable<UserInterest>> GetUserInterest(Guid accountId)
        {
            return await _dbService.GetUserInterest(accountId);
        }

        [HttpPost("AddUserInterest")]
        public async Task<IActionResult> AddUserInterest(UserInterest userInterest)
        {
            var result = await _dbService.AddUserInterest(userInterest);

            if (result.Success)
            {
                var dataResult = await _dbService.GetUserInterest(result.Data!.AccountId);
                return CreatedAtAction("GetUserInterest", new { id = result.Data!.AccountId }, dataResult);
            }

            if (result.ErrorMessage != null && result.ErrorMessage.StartsWith("Internal"))
            {
                return StatusCode(500, new { message = result.ErrorMessage });
            }

            return BadRequest(new { message = result.ErrorMessage });
        }
    }
}
