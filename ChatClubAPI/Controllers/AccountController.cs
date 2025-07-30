using ChatClubAPI.Data;
using ChatClubAPI.Models;
using ChatClubAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly DbService _dbService;
        private readonly JwtTokenService _tokenService;
        private readonly CalculateService _calculateService;

        public AccountController(DbService dbService, JwtTokenService tokenService, CalculateService calculateService)
        {
            _dbService = dbService;
            _tokenService = tokenService;
            _calculateService = calculateService;
        }

        [HttpPost("CreateAccount")]
        public async Task<IActionResult> CreateAccount([FromForm] FileInput files)
        {
            Location? location = await _calculateService.CheckLocation(files.Latitude, files.Longitude);

            if (location is null)
            {
                return NotFound("No nearby location found within the specified radius.");
            }

            Guid newGuid = Guid.NewGuid();
            TokenResponse tokenResponse = _tokenService.GenerateToken(newGuid, "user", "web");

            Account account = new Account()
            {
                UserId = newGuid,
                Username = newGuid.ToString(),
                Password = "",
                Name = files.Name,
                Gender = files.Gender,
                Role = 5,
                CreateDate = DateTime.Now,
                Active = true
            };

            bool createAccount = await _dbService.CreateAccount(account);

            return Ok(tokenResponse);
        }
    }
}
