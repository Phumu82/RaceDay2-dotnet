using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Domain.Entities;

namespace RaceDay.Api.Auth;

// Issues signed JWTs consumed by both the Web API's own [Authorize]
// attributes and the MVC front-end, which forwards the token on every
// call it makes to the API on the user's behalf.
public class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.Key) || _options.Key.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be configured and at least 32 characters. Set it via user-secrets or environment variables, never commit it.");
    }

    public (string Token, DateTime ExpiresAt) CreateToken(Profile profile)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, profile.Id.ToString()),
            new(ClaimTypes.NameIdentifier, profile.Id.ToString()),
            new(ClaimTypes.Name, profile.FullName),
            new(ClaimTypes.Email, profile.Email),
            new(ClaimTypes.Role, profile.Role.ToString()),
            new("security_stamp", profile.SecurityStamp)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
