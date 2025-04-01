using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Licensing;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;
    private readonly MainDbContext _mainDbContext;
    private readonly LicenseProvider _licenseProvider;

    public TokenService(IConfiguration configuration, MainDbContext mainDbContext, LicenseProvider licenseProvider)
    {
        _configuration = configuration;
        _mainDbContext = mainDbContext;
        _licenseProvider = licenseProvider;
    }

    public async Task<(string AccessToken, RefreshToken RefreshToken)> GenerateTokensAsync(User user, Company? company = null)
    {
        // Try to acquire a license and create a session
        var sessionId = string.Empty;
        if (company != null)
        {
            var acquisitionResponse = await _licenseProvider.TryAcquireLicenseAndCreateSessionAsync(user.Id, company.Id);
            if (!acquisitionResponse.Success)
            {
                ValidationContext.Instance.ThrowError("No available licenses.", StatusCodes.Status400BadRequest);
                return default;
            }
            
            sessionId = acquisitionResponse.SessionId;
        }

        // Generate access token with session ID
        var accessToken = GenerateAccessToken(user, company, sessionId);
        
        // Generate refresh token
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, company?.Id);

        return (accessToken, refreshToken);
    }
    
    public string GenerateAccessToken(User user, Company? company, string sessionId)
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
                new Claim(CustomClaimTypes.SessionId, sessionId),
                new Claim(CustomClaimTypes.CompanyId, company.Id.ToString()),
                new Claim(CustomClaimTypes.DatabaseType, ((int)company.DatabaseType).ToString())
            );
        }

        // Get the JWT settings from configuration
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            notBefore: DateTime.Now,
            expires: DateTime.Now.AddMinutes(int.Parse(jwtSettings["ExpiryMinutes"] ?? "15")),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
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
        await _mainDbContext.RefreshTokens.AddAsync(token);
        await _mainDbContext.SaveChangesAsync();

        return token;
    }

    public async Task<(string AccessToken, RefreshToken RefreshToken)> RefreshTokenAsync(string token)
    {
        var refreshToken = await GetRefreshTokenAsync(token);
        if (refreshToken == null || !refreshToken.IsActive)
        {
            ValidationContext.Instance.ThrowError("Invalid refresh token.", StatusCodes.Status400BadRequest);
            return default;
        }

        var currentUser = (await _mainDbContext.Users.FindAsync(refreshToken.UserId))!;

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
            
            currentUser.LastUsedCompanyId = company.Id;
            await _mainDbContext.SaveChangesAsync();
        }
        
        var sessionId = await _licenseProvider.GetCurrentSessionIdAsync(currentUser.Id);
        if (string.IsNullOrEmpty(sessionId))
        {
            ValidationContext.Instance.ThrowError("Invalid session", StatusCodes.Status400BadRequest);
            return default;
        }
        
        // Generate new tokens
        var newAccessToken = GenerateAccessToken(currentUser!, company, sessionId);
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
            .Where(t => t.UserId == userId && !t.IsRevoked && DateTime.UtcNow < t.ExpiresAt)
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