using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using WopiHost.Models;

namespace WopiHost.Services;

public sealed class JwtService : IJwtService
{
    private readonly JwtConfig _config;
    private readonly SigningCredentials _signingCreds;

    public JwtService(IOptions<JwtConfig> options)
    {
        _config = options.Value;
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.Secret));
        _signingCreds = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
    }

    public string JwtTokenGenerator(string userId, string userName)
    {
        var now = DateTimeOffset.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //unique identifier for the token
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), //issued at time for token

            new Claim(JwtRegisteredClaimNames.PreferredUsername, userName)
        };

        var token = new JwtSecurityToken(
            issuer: _config.Issuer,
            audience: _config.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.AddMinutes(_config.ExpiresMinutes).UtcDateTime,
            signingCredentials: _signingCreds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}