using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;
    private readonly MainDbContext _mainDbContext;
    private readonly UserProvider _userProvider;

    public TokenService(IConfiguration configuration, MainDbContext mainDbContext, UserProvider userProvider)
    {
        _configuration = configuration;
        _mainDbContext = mainDbContext;
        _userProvider = userProvider;
    }

    public string GenerateAccessToken(User user, Company? company)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(CustomClaimTypes.UserType, user.UserType.ToString())
        };

        if (company != null)
        {
            claims.Add(
                new Claim(CustomClaimTypes.CompanyId, company.Id.ToString()),
                new Claim(CustomClaimTypes.DatabaseType, ((int)company.DatabaseType).ToString()));
        }

        var token = JwtBearer.CreateToken( 
            o => 
            {
                o.ExpireAt = DateTime.UtcNow.AddHours(1); 
                o.User.Claims.AddRange(claims);
            });

        return token;
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(long userId, long? companyId)
    {
        // Generate a secure random token
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var refreshToken = Convert.ToBase64String(randomBytes);

        // Get refresh token expiry time from configuration
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expiryDays = int.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7");

        // Create refresh token entity
        var token = new RefreshToken
        {
            UserId = userId,
            CompanyId = companyId,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow
        };

        // Save to database
        _mainDbContext.RefreshTokens.Add(token);
        await _mainDbContext.SaveChangesAsync();

        return token;
    }

    public async Task<(string AccessToken, RefreshToken RefreshToken)> RefreshTokenAsync(string token)
    {
        var refreshToken = await GetRefreshTokenAsync(token);

        if (refreshToken == null || !refreshToken.IsActive || refreshToken.UserId != _userProvider.GetCurrentUserId())
        {
            ValidationContext.Instance.ThrowError("Invalid refresh token.", StatusCodes.Status400BadRequest);
            return default;
        }

        var currentUser = await _mainDbContext.Users.FindAsync(refreshToken.UserId);

        // Check if company exists
        Company? company = null;
        if (refreshToken.CompanyId.HasValue)
        {
            company = await _mainDbContext.UserCompanies
                .Where(uc => uc.UserId == refreshToken.UserId && uc.CompanyId == refreshToken.CompanyId)
                .Select(uc => uc.Company)
                .SingleOrDefaultAsync();

            if (company == null)
            {
                ValidationContext.Instance.ThrowError("Company not active or not found", StatusCodes.Status400BadRequest);
                return default;
            }
            
            currentUser!.LastUsedCompanyId = company.Id;
            await _mainDbContext.SaveChangesAsync();
        }
        
        // Generate new tokens
        var newAccessToken = GenerateAccessToken(currentUser!, company);
        var newRefreshToken = await GenerateRefreshTokenAsync(currentUser!.Id, company?.Id);

        refreshToken.UsedAt = DateTime.UtcNow;
        await _mainDbContext.SaveChangesAsync();

        return (newAccessToken, newRefreshToken);
    }

    public async Task RevokeTokenAsync(string token)
    {
        var refreshToken = await GetRefreshTokenAsync(token);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            ValidationContext.Instance.ThrowError("Invalid token", StatusCodes.Status400BadRequest);
            return;
        }

        // Revoke token
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;

        await _mainDbContext.SaveChangesAsync();
    }

    public async Task RevokeAllUserTokensAsync(long userId)
    {
        var activeTokens = await _mainDbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.IsActive)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }

        await _mainDbContext.SaveChangesAsync();
    }

    private async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _mainDbContext.RefreshTokens.SingleOrDefaultAsync(r => r.Token == token);
    }
}