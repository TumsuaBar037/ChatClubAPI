using ChatClubAPI.Data;
using ChatClubAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatClubAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<TokenService> _logger;
        private readonly DbService _dbService;

        public TokenService(IConfiguration configuration, ILogger<TokenService> logger, DbService dbService)
        {
            _config = configuration;
            _logger = logger;
            _dbService = dbService;
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var jwtKey = _config["JwtConfig:Key"] ?? throw new ArgumentNullException("JWT Key not found");
                var key = Encoding.ASCII.GetBytes(jwtKey);

                var audiences = _config.GetSection("JwtConfig:Audience").Get<string[]>()
                       ?? throw new ArgumentNullException("JWT Audiences not found");

                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _config["JwtConfig:Issuer"],
                    ValidateAudience = true,
                    ValidAudiences = audiences,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<TokenResponse> GenerateTokensAsync(Guid accountId, string role, string platform)
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
                new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
                new Claim(ClaimTypes.Name, accountId.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("token_type", "access")
            };

            var refreshClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, role),
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

        public async Task<TokenResponse?> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                // check token in database
                UserToken? storedToken = await _dbService.FindUserTokenByRefreshToken(refreshToken);

                if (storedToken == null)
                {
                    return null;
                }

                bool isValid = ValidateToken(storedToken.RefreshToken);

                if (!isValid)
                {
                    return null;
                }

                // new token
                string aud = GetAudience(storedToken.RefreshToken)?.ToLower() ?? string.Empty;

                if (string.IsNullOrEmpty(aud))
                {
                    _logger.LogWarning("Audience not found in refresh token");
                    return null;
                }

                string platform = aud switch
                {
                    "nearsip.web" => "web",
                    "nearsip.mobile" => "mobile",
                    _ => "web"
                };
                string? role = GetRole(storedToken.RefreshToken);
                var newTokens = await GenerateTokensAsync(storedToken.AccountId, role!, platform);

                // update refresh token in database
                storedToken.RefreshToken = newTokens.RefreshToken;
                _ = await _dbService.UpdateToken(storedToken.Id, storedToken);

                return newTokens;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return null;
            }
        }

        public string? GetNameIdentifier(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var nameId = jwtToken.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier
                                  || c.Type.Contains("nameidentifier"))
                ?.Value;

            return nameId;
        }

        public string? GetAudience(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Audiences.FirstOrDefault();
        }

        public string? GetRole(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role)
                ?.Value;
        }
    }
}
