using ChatClubAPI.Models;
using ChatClubAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatClubAPI.Controllers
{
    public class FileInputModel
    {
        public IFormFile UserProfile { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Gender { get; set; }
        public string GenderPreference { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JwtTokenService _tokenService;

        public AccountController(JwtTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("CreateAccount")]
        public async Task<IActionResult> CreateAccount([FromForm] FileInputModel files)
        {
            Guid newGuid = Guid.NewGuid();

            TokenResponse tokenResponse = _tokenService.GenerateToken(newGuid, "user", "web");

            return Ok("test");
        }
    }
}
