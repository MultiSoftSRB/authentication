using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Services;

namespace MultiSoftSRB.Features.Auth.SwitchCompany;

sealed class Endpoint : Endpoint<Request, Response>
{
    public MainDbContext MainDbContext { get; set; }
    public UserProvider UserProvider { get; set; }
    public TokenService TokenService { get; set; }
    
    public override void Configure()
    {
        Post("auth/switch-company");
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var currentUserId = UserProvider.GetCurrentUserId();
        var company = await MainDbContext.UserCompanies
            .Where(uc => uc.UserId == currentUserId && uc.CompanyId == request.CompanyId)
            .Select(uc => uc.Company)
            .SingleOrDefaultAsync(cancellationToken);
        
        if (company == null)
        {
            AddError("You don't have access to this company");
            await SendErrorsAsync(StatusCodes.Status403Forbidden, cancellationToken);
            return;
        }

        // Get user for token generation
        var user = await MainDbContext.Users.SingleOrDefaultAsync(u => u.Id == currentUserId, cancellationToken: cancellationToken);
        if (user == null)
        {
            AddError("User not found");
            await SendErrorsAsync(StatusCodes.Status404NotFound, cancellationToken);
            return;
        }

        // Update last used company
        user.LastUsedCompanyId = request.CompanyId;
        await MainDbContext.SaveChangesAsync(cancellationToken);
        
        // Generate new JWT token with new company context
        var accessToken = TokenService.GenerateAccessToken(user, company);
        var refreshToken = await TokenService.GenerateRefreshTokenAsync(user.Id, company.Id);

        // Set refresh token as a HTTP-only cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = refreshToken.ExpiresAt,
            Secure = HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
        };
    
        HttpContext.Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        
        // Return response
        await SendOkAsync(new Response
        {
            AccessToken = accessToken
        }, cancellation: cancellationToken);
    }
}