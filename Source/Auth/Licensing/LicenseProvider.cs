using System.Security.Cryptography;
using MultiSoftSRB.Database.Main;
using Microsoft.EntityFrameworkCore;
using ZiggyCreatures.Caching.Fusion;

namespace MultiSoftSRB.Auth.Licensing;

public class LicenseProvider
{
    private readonly IFusionCache _cache;
    private readonly MainDbContext _mainDbContext;
    private readonly FusionCacheEntryOptions _cacheOptions;

    // Cache key prefixes
    private const string CompanyLicenseKeyPrefix = "CompanyLicense_";
    private const string UserSessionKeyPrefix = "UserSession_";

    public LicenseProvider(IFusionCache cache, MainDbContext mainDbContext, IConfiguration configuration)
    {
        _cache = cache;
        _mainDbContext = mainDbContext;
        _cacheOptions = new FusionCacheEntryOptions
        {
            Duration = TimeSpan.FromMinutes(configuration.GetValue<short>("LicensingSettings:SessionInactivityTimeoutMinutes"))
        };
    }

    /// <summary>
    /// Generates a unique session ID
    /// </summary>
    public string GenerateSessionId()
    {
        // Generate a cryptographically strong random session ID
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
    
    /// <summary>
    /// Attempts to acquire a license and create a new session for a user
    /// </summary>
    public async Task<LicenseAcquisitionResult> TryAcquireLicenseAndCreateSessionAsync(long userId, long companyId)
    {
        // Check if there are available licenses for this company
        if (!await HasAvailableLicensesAsync(companyId))
            return new() { Success = false };

        // Generate a new session ID
        var sessionId = GenerateSessionId();
        
        // Remove previous session if exists
        await RemovePreviousUserSessionAsync(userId);

        // Decrement available license count for the company
        await DecrementAvailableLicensesAsync(companyId);

        // Store the new session
        await StoreSessionAsync(userId, companyId, sessionId);
        
        return new() { Success = true, SessionId = sessionId };
    }

    /// <summary>
    /// Validates if a session is still active and updates its last activity timestamp
    /// </summary>
    public async Task<bool> ValidateAndRefreshSessionAsync(long userId, long companyId, string sessionId)
    {
        var cacheKey = $"{UserSessionKeyPrefix}{userId}";

        // Get session info from cache
        var session = await _cache.GetOrDefaultAsync<UserSessionInfo>(cacheKey);
        if (session == null)
            return false;
        
        // Validate session details
        if (session.SessionId != sessionId)
            return false;

        // Update last activity time
        session.LastActivity = DateTime.UtcNow;
        session.CompanyId = companyId;
        
        // Refresh the cache with sliding expiration
        await _cache.SetAsync(cacheKey, session, _cacheOptions);
        
        return true;
    }

    /// <summary>
    /// Explicitly releases a license when a user logs out
    /// </summary>
    public async Task ReleaseLicenseAsync(long userId, long companyId, string sessionId)
    {
        var cacheKey = $"{UserSessionKeyPrefix}{userId}";
        var session = await _cache.GetOrDefaultAsync<UserSessionInfo>(cacheKey);
        
        // Only release if the session IDs match
        if (session != null && session.SessionId == sessionId)
        {
            // Remove the session
            await _cache.RemoveAsync(cacheKey);
            
            // Increment available license count
            await IncrementAvailableLicensesAsync(companyId);
        }
    }

    /// <summary>
    /// Removes a user's previous session if it exists
    /// </summary>
    private async Task RemovePreviousUserSessionAsync(long userId)
    {
        var cacheKey = $"{UserSessionKeyPrefix}{userId}";
        var existingSession = await _cache.GetOrDefaultAsync<UserSessionInfo>(cacheKey);
        
        if (existingSession != null)
        {
            // Increment available licenses for the previous company
            await IncrementAvailableLicensesAsync(existingSession.CompanyId);
            
            // Remove the existing session
            await _cache.RemoveAsync(cacheKey);
        }
    }

    /// <summary>
    /// Stores a new user session
    /// </summary>
    private async Task StoreSessionAsync(long userId, long companyId, string sessionId)
    {
        var cacheKey = $"{UserSessionKeyPrefix}{userId}";
        var session = new UserSessionInfo
        {
            UserId = userId,
            CompanyId = companyId,
            SessionId = sessionId,
            CreatedAt = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow
        };
        
        await _cache.SetAsync(cacheKey, session, _cacheOptions);
    }

    /// <summary>
    /// Gets the number of total and available licenses for a company
    /// </summary>
    public async Task<LicenseInfo> GetLicenseInfoAsync(long companyId)
    {
        // Get total licenses from database
        var company = await _mainDbContext.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == companyId);

        if (company == null)
        {
            return new LicenseInfo
            {
                TotalLicenses = 0,
                AvailableLicenses = 0
            };
        }

        var totalLicenses = company.LicenseCount;
        var availableLicenses = await GetAvailableLicensesAsync(companyId);

        return new LicenseInfo
        {
            TotalLicenses = totalLicenses,
            AvailableLicenses = availableLicenses
        };
    }

    #region License Management Helper Methods

    /// <summary>
    /// Gets the current session ID for a user
    /// </summary>
    public async Task<string?> GetCurrentSessionIdAsync(long userId)
    {
        var cacheKey = $"{UserSessionKeyPrefix}{userId}";
        
        // Get session info from cache
        var session = await _cache.GetOrDefaultAsync<UserSessionInfo>(cacheKey);
        
        return session?.SessionId;
    }
        
    private async Task<bool> HasAvailableLicensesAsync(long companyId)
    {
        await InitializeLicenseCountAsync(companyId);
        
        var availableLicenses = await GetAvailableLicensesAsync(companyId);
        return availableLicenses > 0;
    }
    
    /// <summary>
    /// Initializes the license count in cache from the database if not already present
    /// </summary>
    public async Task InitializeLicenseCountAsync(long companyId)
    {
        var cacheKey = $"{CompanyLicenseKeyPrefix}{companyId}";
        
        // Check if license count is already in cache
        var valueExists = await _cache.TryGetAsync<short>(cacheKey);
        if (valueExists.HasValue)
            return;

        // Get license count from database
        var company = await _mainDbContext.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == companyId);

        if (company == null)
            return;
        
        // Initialize available licenses to total licenses
        await _cache.SetAsync(cacheKey, company.LicenseCount, _cacheOptions);
    }
    
    private async Task<short> GetAvailableLicensesAsync(long companyId)
    {
        var cacheKey = $"{CompanyLicenseKeyPrefix}{companyId}";
        
        // Try to get from cache
        var availableLicenses = await _cache.TryGetAsync<short>(cacheKey);
        if (!availableLicenses.HasValue)
        {
            // If not in cache, initialize from database
            await InitializeLicenseCountAsync(companyId);
            availableLicenses = await _cache.GetOrDefaultAsync<short>(cacheKey);
        }
        
        return availableLicenses.Value;
    }
    
    private async Task DecrementAvailableLicensesAsync(long companyId)
    {
        var cacheKey = $"{CompanyLicenseKeyPrefix}{companyId}";
        var availableLicenses = await GetAvailableLicensesAsync(companyId);
        if (availableLicenses <= 0)
            throw new InvalidOperationException($"No available licenses for company {companyId}");
        
        await _cache.SetAsync(cacheKey, (short)(availableLicenses - 1), _cacheOptions);
    }
    
    private async Task IncrementAvailableLicensesAsync(long companyId)
    {
        var cacheKey = $"{CompanyLicenseKeyPrefix}{companyId}";
        var availableLicenses = await GetAvailableLicensesAsync(companyId);
        
        // Get total licenses to ensure we don't exceed
        var company = await _mainDbContext.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == companyId);
        
        if (company == null)
            return;
        
        // Ensure we don't exceed total licenses
        var newCount = Math.Min((short)(availableLicenses + 1), company.LicenseCount);
        
        await _cache.SetAsync(cacheKey, newCount, _cacheOptions);
    }

    #endregion
}