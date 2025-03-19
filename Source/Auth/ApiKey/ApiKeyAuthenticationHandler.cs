using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MultiSoftSRB.Database.Main;
using ZiggyCreatures.Caching.Fusion;

namespace MultiSoftSRB.Auth.ApiKey;

sealed class ApiKeyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, 
                        IFusionCache cache, MainDbContext mainDbContext)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    internal const string SchemeName = "ApiKey";
    internal const string HeaderName = "x-api-key";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Skip authentication if endpoint allows anonymous access
        if (Context.GetEndpoint()?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null)
            return AuthenticateResult.NoResult();

        // Look for API key in the header. If it does not exist, continue with auth (JWT will try to auth)
        if (!Request.Headers.TryGetValue(HeaderName, out var apiKeyHeaderValues))
            return AuthenticateResult.NoResult();
        
        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
        if (string.IsNullOrEmpty(providedApiKey))
            return AuthenticateResult.Fail("API key is missing");

        // Try to validate the API key
        var validationResult = await ValidateApiKeyAsync(providedApiKey);
        if (!validationResult.IsValid)
            return AuthenticateResult.Fail(validationResult.FailureReason ?? "Invalid API key");

        // Create claims principal from validation result
        var claims = new List<Claim>
        {
            new(CustomClaimTypes.CompanyId, validationResult.CompanyId.ToString())
        };
        
        // Add resource permissions as claims
        foreach (var permission in validationResult.Permissions)
        {
            claims.Add(new Claim(CustomClaimTypes.ResourcePermission, permission));
        }
        
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        
        return AuthenticateResult.Success(ticket);
    }
    
    private async Task<ApiKeyValidationResult> ValidateApiKeyAsync(string providedApiKey)
    {
        var cacheKey = $"ApiKey_{providedApiKey}";
        
        // Setup FusionCache options
        var options = new FusionCacheEntryOptions
        {
            Duration = TimeSpan.FromMinutes(15),
            Priority = CacheItemPriority.High
        };
        
        // Get from cache or compute if not found
        return await cache.GetOrSetAsync(
            cacheKey,
            async _ => 
            {
                // Hash the provided key
                var hashedProvidedKey = HashApiKey(providedApiKey);
            
                var apiKey = await mainDbContext.ApiKeys
                    .Where(k => k.KeyHash == hashedProvidedKey)
                    .Include(k => k.Permissions)
                    .FirstOrDefaultAsync();
            
                if (apiKey == null)
                {
                    return new ApiKeyValidationResult 
                    { 
                        IsValid = false, 
                        FailureReason = "Invalid API key" 
                    };
                }
            
                // Check if the key has expired
                if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow)
                {
                    return new ApiKeyValidationResult 
                    { 
                        IsValid = false, 
                        FailureReason = "API key has expired" 
                    };
                }
            
                // Create validation result
                return new ApiKeyValidationResult
                {
                    IsValid = true,
                    ApiKeyId = apiKey.Id,
                    CompanyId = apiKey.CompanyId,
                    UserId = apiKey.CreatedBy,
                    Permissions = apiKey.Permissions.Select(p => p.ResourcePermissionCode).ToArray()
                };
            },
            options);
    }
    
    // Helper method to hash an API key for comparison
    private string HashApiKey(string apiKey)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(bytes);
    }
}