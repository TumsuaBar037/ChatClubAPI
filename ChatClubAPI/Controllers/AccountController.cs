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
        private readonly JwtTokenService _tokenService;

        public AccountController(JwtTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("CreateAccount")]
        public async Task<IActionResult> CreateAccount([FromForm] FileInput files)
        {
            Guid newGuid = Guid.NewGuid();
            TokenResponse tokenResponse = _tokenService.GenerateToken(newGuid, "user", "web");

            return Ok(tokenResponse);
        }
    }
}
