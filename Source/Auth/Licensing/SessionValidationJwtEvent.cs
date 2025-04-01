using Microsoft.AspNetCore.Authentication.JwtBearer;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Auth.Licensing;

public class SessionValidationJwtEvents : JwtBearerEvents
{
    public SessionValidationJwtEvents()
    {
        OnTokenValidated = ValidateSessionAsync;
    }

    private async Task ValidateSessionAsync(TokenValidatedContext context)
    {
        var sessionLicensingService = context.HttpContext.RequestServices.GetRequiredService<LicenseProvider>();
        var userProvider = context.HttpContext.RequestServices.GetRequiredService<UserProvider>();

        // Skip session validation for API key authentication
        if (context.Principal?.HasClaim(c => c.Type == CustomClaimTypes.ApiKeyId) == true)
            return;

        // Skip session validation for SuperAdmin and Consultant
        var userType = userProvider.GetCurrentUserType();
        if (userType == UserType.SuperAdmin || userType == UserType.Consultant)
            return;

        // Extract session ID (jti claim)
        var sessionIdClaim = context.Principal?.FindFirst("jti");
        if (sessionIdClaim == null)
        {
            context.Fail("Session validation failed: Missing session ID");
            return;
        }

        // Extract user ID and company ID
        var userId = userProvider.GetCurrentUserIdNullable();
        var companyId = userProvider.GetCurrentCompanyId();
            
        if (!userId.HasValue || !companyId.HasValue)
        {
            context.Fail("Session validation failed: Missing required claims");
            return;
        }

        // Validate session is still active
        var isSessionValid = await sessionLicensingService.ValidateAndRefreshSessionAsync(userId.Value, companyId.Value, sessionIdClaim.Value);
        if (!isSessionValid)
        {
            context.Fail("Session validation failed: Invalid session");
            return;
        }
    }
}