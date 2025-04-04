using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MultiSoftSRB.Auth.ApiKey;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Auth.Licensing;

public class SessionValidationMiddleware
{
    private readonly RequestDelegate _next;

    public SessionValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, LicenseProvider licenseProvider, UserProvider userProvider, EndpointDataSource endpointDataSource)
    {
        // Check if endpoint is marked as "allow anonymous"
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null)
        {
            await _next(context);
            return;
        }

        // Skip validation if user is not authenticated
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        // Skip session validation for API key authentication
        if (context.User.Identity?.AuthenticationType == ApiKeyAuthenticationHandler.SchemeName)
        {
            await _next(context);
            return;
        }

        // Skip session validation if user is SuperAdmin or Consultant
        var userType = userProvider.GetCurrentUserType();
        if (userType == UserType.SuperAdmin || userType == UserType.Consultant)
        {
            await _next(context);
            return;
        }

        // Extract session ID (jti claim)
        var sessionIdClaim = context.User.FindFirst(CustomClaimTypes.SessionId);
        if (sessionIdClaim == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        var sessionId = sessionIdClaim.Value;

        // Extract user ID and company ID - using our existing UserProvider
        var userId = userProvider.GetCurrentUserId();
        var companyId = userProvider.GetCurrentCompanyId();

        // Validate session is still active
        var isSessionValid = await licenseProvider.ValidateAndRefreshSessionAsync(userId, companyId, sessionId);
        if (!isSessionValid)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        // Continue with the request
        await _next(context);
    }
}