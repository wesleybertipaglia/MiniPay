using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Authentication.Core.Dto;
using Authentication.Core.Interface;
using Authentication.Core.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Application.Service;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public TokenDto GenerateJwtToken(User user)
    {
        var secretKey = configuration["Jwt:SecretKey"]
                        ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
        
        var issuer = configuration["Jwt:Issuer"]
                     ?? throw new InvalidOperationException("JWT Issuer is not configured.");

        var audience = configuration["Jwt:Audience"]
                       ?? throw new InvalidOperationException("JWT Audience is not configured.");

        var expires = DateTime.UtcNow.AddHours(1);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );
        
        return new TokenDto(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}