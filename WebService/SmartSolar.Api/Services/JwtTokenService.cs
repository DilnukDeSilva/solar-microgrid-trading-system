/*
 * File: JwtTokenService.cs
 * Description: Builds HMAC-signed JWTs from configuration (issuer, audience, signing key).
 * Author: Member 1
 * Created: 20/09/2026
 *
 * JWT structure and bearer authentication follow Microsoft docs:
 * https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn
 */

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartSolar.Api.Configuration;
using SmartSolar.Api.Models;

namespace SmartSolar.Api.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    // Loads JWT settings that must come from configuration, not source code.
    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    // Creates a signed token containing sub, role and nic (prosumers only).
    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);
        var claims = new List<Claim>
        {
            new("sub", user.Id),
            new("role", user.Role)
        };

        if (!string.IsNullOrWhiteSpace(user.Nic))
        {
            claims.Add(new Claim("nic", user.Nic));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return (token, expiresAt);
    }
}
