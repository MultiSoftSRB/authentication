using System.Security.Claims;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Licensing;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Services;

namespace MultiSoftSRB.Features.Auth.Logout;

sealed class LogoutEndpoint : EndpointWithoutRequest
{
    public TokenService TokenService { get; set; }
    public LicenseProvider LicenseProvider { get; set; }
    public UserProvider UserProvider { get; set; }
    public MainDbContext MainDbContext { get; set; }

    public override void Configure()
    {
        Post("auth/logout");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        // Get the current user's ID from the claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            AddError("User not authenticated");
            await SendErrorsAsync(StatusCodes.Status401Unauthorized, cancellationToken);
            return;
        }

        var userId = long.Parse(userIdClaim.Value);

        // Get the session ID from claims
        var sessionIdClaim = User.FindFirst(CustomClaimTypes.SessionId);
        if (sessionIdClaim == null)
        {
            AddError("Invalid session");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }

        var sessionId = sessionIdClaim.Value;

        // Get the company ID from claims
        var companyClaim = User.FindFirst(CustomClaimTypes.CompanyId);
        long companyId = companyClaim != null 
            ? long.Parse(companyClaim.Value) 
            : 0;

        // Revoke all user tokens
        await TokenService.RevokeAllUserTokensAsync(userId);

        // Release the license and remove the session
        await LicenseProvider.ReleaseLicenseAsync(userId, companyId, sessionId);

        // Remove the refresh token cookie
        HttpContext.Response.Cookies.Delete("refreshToken");

        await SendOkAsync(cancellationToken);
    }
}