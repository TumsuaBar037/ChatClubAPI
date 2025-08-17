using Azure.Core;
using ChatClubAPI.Data;
using ChatClubAPI.Extensions;
using ChatClubAPI.Models;
using ChatClubAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChatClubAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly DbService _dbService;
        private readonly ITokenService _tokenService;
        private readonly CalculateService _calculateService;
        private readonly IFileService _fileService;

        public AccountController(DbService dbService, ITokenService tokenService, CalculateService calculateService, IFileService fileService)
        {
            _dbService = dbService;
            _tokenService = tokenService;
            _calculateService = calculateService;
            _fileService = fileService;
        }

        [AllowAnonymous]
        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            return Ok("Test successful");
        }

        [AllowAnonymous]
        [HttpPost("CreateAccount")]
        public async Task<IActionResult> CreateAccount([FromBody] FileInput files)
        {
            Location? location = await _calculateService.CheckLocation(files.Latitude, files.Longitude);

            if (location is null)
            {
                return NotFound("No nearby location found within the specified radius.");
            }

            DateTime timestamp = DateTime.Now;
            Guid newGuid = Guid.NewGuid();
            TokenResponse tokenResponse = await _tokenService.GenerateTokensAsync(newGuid, "user", "web");

            // event.
            // 0 = login.
            UserLocation userLocation = new UserLocation
            {
                AccountId = newGuid,
                Latitude = location.Latitude,
                Longtitude = location.Longtitude,
                LocationName = location.Name,
                Timestamp = timestamp,
                Event = 0
            };

            UserProfile userProfile = new UserProfile()
            {
                AccountId = newGuid,
                Age = null,
                AboutMe = "",
                AvatarUrl = ""
            };

            UserToken userToken = new UserToken()
            {
                AccountId = newGuid,
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                CreateAt = timestamp
            };

            Account account = new Account()
            {
                Id = newGuid,
                Username = newGuid.ToString(),
                Password = "",
                Name = files.Name,
                Gender = files.Gender,
                Role = 5,
                CreateDate = timestamp,
                Active = true,
                UserLocations = new List<UserLocation> { userLocation },
                UserProfiles = new List<UserProfile> { userProfile },
                UserTokens = new List<UserToken> { userToken }
            };

            bool createAccount = await _dbService.CreateAccount(account);

            if (!createAccount)
            {
                return StatusCode(500, "Failed to create account. Please try again later.");
            }

            _ = await _fileService.SaveUserImageAsync(files.UserProfile!, newGuid);


            // set cookies into fontend

            var locationDataObj = new
            {
                LocationId = location.Id,
                LocationName = location.Name,
                Latitude = location.Latitude,
                Longtitude = location.Longtitude
            };

            string locationData = JsonConvert.SerializeObject(locationDataObj);

            Response.Cookies.Append("NEARSIP_ACCESS", tokenResponse!.AccessToken!, CookieOptionsExtensions.DefaultOptions());
            Response.Cookies.Append("NEARSIP_REFRESH", tokenResponse!.RefreshToken!, CookieOptionsExtensions.DefaultOptions());
            Response.Cookies.Append("NEARSIP_LOCATION", locationData, CookieOptionsExtensions.DefaultOptions());
            
            return NoContent();
        }

        [HttpGet("GetAccount/{id}")]
        public async Task<IActionResult> GetAccount(Guid id)
        {
            var account = await _dbService.GetAccount(id);
            if (account is null)
            {
                return NotFound("Account not found.");
            }

            return Ok(account);
        }

        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
        {
            try
            {
                var result = _tokenService.RefreshTokenAsync(request.RefreshToken!);


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

        [AllowAnonymous]
        [HttpPost("LoginCheck")]
        public async Task<IActionResult> LoginCheck([FromBody] LoginRequest request)
        {
            Account? account = await _dbService.FindAccount(request.Username!, request.Password!);

            if (account is null)
            {
                return Unauthorized("Invalid credentials.");
            }

            Location? location = await _calculateService.CheckLocation(request.Latitude, request.Longitude);

            if (location is null)
            {
                return NotFound("No nearby location found within the specified radius.");
            }

            UserToken? userToken = await _dbService.GetUserToken(account!.Id);

            TokenResponse? newToken = await _tokenService.RefreshTokenAsync(userToken!.RefreshToken!);

            if (newToken is null) 
            {
                return Unauthorized("Invalid refresh token.");
            }

            // set cookies into fontend

            var locationDataObj = new
            {
                LocationId = location.Id,
                LocationName = location.Name,
                Latitude = location.Latitude,
                Longtitude = location.Longtitude
            };

            string locationData = JsonConvert.SerializeObject(locationDataObj);

            Response.Cookies.Append("NEARSIP_ACCESS", newToken!.AccessToken!, CookieOptionsExtensions.DefaultOptions());
            Response.Cookies.Append("NEARSIP_REFRESH", newToken!.RefreshToken!, CookieOptionsExtensions.DefaultOptions());
            Response.Cookies.Append("NEARSIP_LOCATION", locationData, CookieOptionsExtensions.DefaultOptions());

            return Ok("Login success");
        }

        [HttpGet("uploads/images/users/{userId}")]
        public IActionResult GetUserImage(string userId)
        {
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "images", "users");

            // หาไฟล์ที่มี userId เป็นชื่อไฟล์
            var extensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            foreach (var ext in extensions)
            {
                var filePath = Path.Combine(uploadsPath, $"{userId}{ext}");
                if (System.IO.File.Exists(filePath))
                {
                    var contentType = GetContentType(ext);
                    var fileBytes = System.IO.File.ReadAllBytes(filePath);
                    return File(fileBytes, contentType);
                }
            }

            // ถ้าไม่เจอไฟล์ ส่ง default image หรือ 404
            return NotFound();
        }

        private string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}
