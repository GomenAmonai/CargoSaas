using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cargo.Core;
using Cargo.Core.Entities;
using Cargo.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Cargo.Infrastructure.Services;

/// <summary>
/// Сервис для работы с JWT токенами
/// </summary>
public class JwtService : IJwtService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtService(IConfiguration configuration)
    {
        _secretKey = configuration["Jwt:SecretKey"] 
            ?? throw new InvalidOperationException("Jwt:SecretKey is not configured");
        _issuer = configuration["Jwt:Issuer"] ?? "CargoAPI";
        _audience = configuration["Jwt:Audience"] ?? "CargoClient";
        _expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? AppConstants.Jwt.DefaultExpirationMinutes.ToString());
    }

    public string GenerateToken(AppUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        // TenantId (если есть)
        if (user.TenantId.HasValue)
        {
            claims.Add(new Claim(AppConstants.Jwt.TenantIdClaimType, user.TenantId.Value.ToString()));
        }

        // TelegramId (если есть)
        if (user.TelegramId.HasValue)
        {
            claims.Add(new Claim(AppConstants.Jwt.TelegramIdClaimType, user.TelegramId.Value.ToString()));
        }

        // FirstName
        if (!string.IsNullOrEmpty(user.FirstName))
        {
            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
        }

        // Username или Email
        if (!string.IsNullOrEmpty(user.UserName))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
        }

        // Email (для Managers)
        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secretKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
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
}
