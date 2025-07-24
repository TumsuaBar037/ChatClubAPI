using ChatClubAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatClubAPI.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;

        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        public TokenResponse GenerateToken(Guid userid, string role, string platform)
        {
            var accessTokenExpires = DateTime.UtcNow.AddHours(_config.GetValue<int>("JwtConfig:TokenValidityHours"));
            var refreshTokenExpires = accessTokenExpires.AddDays(7);

            var audience = platform.ToLower() switch
            {
                "web" => "Nearsip.Web",
                "mobile" => "Nearsip.Mobile",
                _ => "Nearsip.Web"
            };

            var accessClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userid.ToString()),
                new Claim(ClaimTypes.Name, userid.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("token_type", "access")
            };

            var refreshClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userid.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("token_type", "refresh")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtConfig:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var audiences = _config.GetSection("JwtConfig:Audience").Get<string[]>();
            var issuer = _config["JwtConfig:Issuer"];

            var accessToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: accessClaims,
                expires: accessTokenExpires,
                signingCredentials: creds
            );

            var refreshToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: refreshClaims,
                expires: refreshTokenExpires,
                signingCredentials: creds
            );

            var tokenHandler = new JwtSecurityTokenHandler();

            return new TokenResponse
            {
                AccessToken = tokenHandler.WriteToken(accessToken),
                RefreshToken = tokenHandler.WriteToken(refreshToken),
                AccessTokenExpires = accessTokenExpires,
                RefreshTokenExpires = refreshTokenExpires
            };
        }
    }
}
