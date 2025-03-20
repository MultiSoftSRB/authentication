using MultiSoftSRB.Services;

namespace MultiSoftSRB.Features.Auth.RefreshToken;

sealed class Endpoint : EndpointWithoutRequest<Response>
{
    public TokenService TokenService { get; set; }
    
    public override void Configure()
    {
        Get("auth/refresh-token");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        // Get refresh token from cookie
        var refreshToken = HttpContext.Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            AddError("Refresh token is required");
            await SendErrorsAsync(StatusCodes.Status401Unauthorized, cancellationToken);
            return;
        }
        
        // Exchange refresh token for new tokens
        var (accessToken, newRefreshToken) = await TokenService.RefreshTokenAsync(refreshToken);

        // Set the new refresh token in a cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = newRefreshToken.ExpiresAt,
            Secure = HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Strict
        };
                
        HttpContext.Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);
                
        // Return the new access token
        await SendOkAsync(new Response
        {
            AccessToken = accessToken,
        }, cancellation: cancellationToken);
    }
}