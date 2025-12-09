using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AiGroupChat.Application.Interfaces;
using AiGroupChat.Domain.Entities;
using AiGroupChat.Infrastructure.Configuration;
using AiGroupChat.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AiGroupChat.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public TokenService(ApplicationDbContext context, IOptions<JwtSettings> jwtSettings)
    {
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateAccessToken(User user)
    {
        List<Claim> claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new("display_name", user.DisplayName)
        };

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: GetAccessTokenExpiration(),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        RefreshToken refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            Revoked = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return token;
    }

    public async Task<string?> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        RefreshToken? token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => 
                t.Token == refreshToken && 
                !t.Revoked && 
                t.ExpiresAt > DateTime.UtcNow, 
                cancellationToken);

        return token?.UserId;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        RefreshToken? token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);

        if (token != null)
        {
            token.Revoked = true;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeAllUserRefreshTokensAsync(string userId, CancellationToken cancellationToken = default)
    {
        List<RefreshToken> tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.Revoked)
            .ToListAsync(cancellationToken);

        foreach (RefreshToken token in tokens)
        {
            token.Revoked = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public DateTime GetAccessTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);
    }
}