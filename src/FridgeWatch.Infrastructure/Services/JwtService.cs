using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using FridgeWatch.Application.Interfaces;

namespace FridgeWatch.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(int userId, string username, string email)
    {
        var key = _configuration["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:Key");
        var issuer = _configuration["Jwt:Issuer"] ?? "FridgeWatch";
        var audience = _configuration["Jwt:Audience"] ?? "FridgeWatchUsers";
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "1440");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int GetUserIdFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("无效的令牌");
        }

        return userId;
    }
}
