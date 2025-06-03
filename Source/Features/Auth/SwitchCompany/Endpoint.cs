using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;
using MultiSoftSRB.Entities.Main.Enums;
using MultiSoftSRB.Services;

namespace MultiSoftSRB.Features.Auth.SwitchCompany;

sealed class Endpoint : Endpoint<Request, Response>
{
    public MainDbContext MainDbContext { get; set; }
    public UserProvider UserProvider { get; set; }
    
    public CompanyProvider CompanyProvider { get; set; }
    public TokenService TokenService { get; set; }
    
    public override void Configure()
    {
        Post("auth/switch-company");
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var currentUserId = UserProvider.GetCurrentUserId();
        var user = await MainDbContext.Users.SingleOrDefaultAsync(u => u.Id == currentUserId, cancellationToken: cancellationToken);
        if (user == null)
        {
            AddError("User not found");
            await SendErrorsAsync(StatusCodes.Status404NotFound, cancellationToken);
            return;
        }

        Company? company;
        if (user.UserType is UserType.SuperAdmin or UserType.Consultant)
            company = await MainDbContext.Companies.SingleOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken);
        else
            company = await MainDbContext.UserCompanies
                .Where(uc => uc.UserId == currentUserId && uc.CompanyId == request.CompanyId)
                .Select(uc => uc.Company)
                .SingleOrDefaultAsync(cancellationToken);
        
        if (company == null)
        {
            AddError("You don't have access to this company");
            await SendErrorsAsync(StatusCodes.Status403Forbidden, cancellationToken);
            return;
        }

        // Update last used company
        user.LastUsedCompanyId = request.CompanyId;
        await MainDbContext.SaveChangesAsync(cancellationToken);
        
        // Generate new JWT token with new company context
        var tokens = await TokenService.GenerateTokensAsync(user, company);
        
        // After we generate tokens, double check if there are any errors and return immediatelly to prevent problems
        ThrowIfAnyErrors();

        // Set refresh token as a HTTP-only cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = tokens.RefreshToken.ExpiresAt,
            Secure = HttpContext.Request.IsHttps,
            SameSite = SameSiteMode.Strict,
        };
    
        HttpContext.Response.Cookies.Append("refreshToken", tokens.RefreshToken.Token, cookieOptions);
        
        // Return response
        var response = new Response
        {
            AccessToken = tokens.AccessToken,
            PagePermissions = await UserProvider.GetPagePermissionsAsync(user.Id, company.Id)
        };
        
        await SendOkAsync(response, cancellationToken);
    }
}