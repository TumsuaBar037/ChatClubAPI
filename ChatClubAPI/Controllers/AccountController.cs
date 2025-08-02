using ChatClubAPI.Data;
using ChatClubAPI.Models;
using ChatClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ChatClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly DbService _dbService;
        private readonly JwtTokenService _tokenService;
        private readonly CalculateService _calculateService;
        private readonly IFileService _fileService;

        public AccountController(DbService dbService, JwtTokenService tokenService, CalculateService calculateService, IFileService fileService)
        {
            _dbService = dbService;
            _tokenService = tokenService;
            _calculateService = calculateService;
            _fileService = fileService;
        }

        [AllowAnonymous]
        [HttpPost("CreateAccount")]
        public async Task<IActionResult> CreateAccount([FromForm] FileInput files)
        {
            Location? location = await _calculateService.CheckLocation(files.Latitude, files.Longitude);

            if (location is null)
            {
                return NotFound("No nearby location found within the specified radius.");
            }

            DateTime timestamp = DateTime.Now;
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
                CreateDate = timestamp,
                Active = true
            };

            bool createAccount = await _dbService.CreateAccount(account);

            if (!createAccount)
            {
                return StatusCode(500, "Failed to create account. Please try again later.");
            }

            UserLocation userLocation = new UserLocation
            {
                UserId = newGuid,
                Latitude = location.Latitude,
                Longtitude = location.Longtitude,
                LocationName = location.Name,
                Timestamp = timestamp,
                Event = 0
            };

            bool createUserLocation = await _dbService.CreateUserLocation(userLocation);
            bool saved = await _fileService.SaveUserImageAsync(files.UserProfile, newGuid);

            return Ok(new SendLogin
            {
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                LocationId = location.LocationId,
                LocationName = location.Name,
                Latitude = location.Latitude,
                Longtitude = location.Longtitude
            });
        }

        [HttpGet("GetAccount/{id}")]
        public async Task<IActionResult> GetAccount(Guid id)
        {
            return Ok(await _dbService.GetAccount(id));
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            try
            {
                var result = _tokenService.RefreshToken(request.RefreshToken!);
                return Ok(result);
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Something went wrong." });
            }
        }
    }
}
