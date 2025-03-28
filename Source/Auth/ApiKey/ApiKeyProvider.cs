namespace MultiSoftSRB.Auth.ApiKey;

public class ApiKeyProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public ApiKeyProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public ApiKeyInfo? GetCurrentApiKey()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;
            
        // Check if the current authentication is using our API key scheme
        var isApiKeyAuth = httpContext.User.Identity?.AuthenticationType == ApiKeyAuthenticationHandler.SchemeName;
        if (!isApiKeyAuth)
            return null;
            
        // Parse the API Key ID from claims
        var apiKeyIdClaim = httpContext.User.FindFirst(CustomClaimTypes.ApiKeyId);
        if (apiKeyIdClaim == null || !long.TryParse(apiKeyIdClaim.Value, out var apiKeyId))
            return null;
            
        // Get company ID from claims
        var companyIdClaim = httpContext.User.FindFirst(CustomClaimTypes.CompanyId);
        if (companyIdClaim == null || !long.TryParse(companyIdClaim.Value, out var companyId))
            return null;
        
        // Get permissions from claims
        var permissions = httpContext.User
            .FindAll(CustomClaimTypes.ResourcePermission)
            .Select(c => c.Value)
            .ToArray();
            
        return new ApiKeyInfo
        {
            Id = apiKeyId,
            CompanyId = companyId,
            Permissions = permissions
        };
    }
}