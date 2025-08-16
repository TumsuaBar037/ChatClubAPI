using ChatClubAPI.Models;
using System.Security.Claims;

namespace ChatClubAPI.Services
{
    public interface ITokenService
    {
        //Task<ClaimsPrincipal> DecodeToken(string token);
        Task<TokenResponse?> RefreshTokenAsync(string refreshToken);
        Task<TokenResponse> GenerateTokensAsync(Guid userid, string role, string platform);
        bool ValidateToken(string token);
    }
}
