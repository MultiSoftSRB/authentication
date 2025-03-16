using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;
using MultiSoftSRB.Entities.Main.Enums;
using MultiSoftSRB.Services;

namespace MultiSoftSRB.Features.Auth.Login;

sealed class Endpoint : Endpoint<Request, Response>
{
    public UserManager<User> UserManager { get; set; }
    public SignInManager<User> SignInManager { get; set; }
    public MainDbContext DbContext { get; set; }
    public TokenService TokenService { get; set; }
    
    public override void Configure()
    {
        Post("auth/login");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await UserManager.FindByNameAsync(request.Username);
        if (user == null)
        {
            AddError("Invalid email or password");
            await SendErrorsAsync(StatusCodes.Status401Unauthorized, cancellationToken);
            return;
        }

        // Check password
        var result = await SignInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            AddError("Invalid email or password");
            await SendErrorsAsync(StatusCodes.Status401Unauthorized, cancellationToken);
            return;
        }
        
        Company? company = null;
        if (user.UserType != UserType.SuperAdmin)
        {
            if (user.LastUsedCompanyId == null || !await DbContext.Companies.AnyAsync(c => c.Id == user.LastUsedCompanyId, cancellationToken: cancellationToken))
            {
                var firstCompanyId = await DbContext.UserCompanies
                    .Where(uc => uc.UserId == user.Id)
                    .Select(uc => (long?)uc.CompanyId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (firstCompanyId == null)
                {
                    AddError("You don't have access to any active companies");
                    await SendErrorsAsync(StatusCodes.Status403Forbidden, cancellationToken);
                    return;
                }
                
                user.LastUsedCompanyId = firstCompanyId.Value;
            }
            
            company = await DbContext.Companies.FirstOrDefaultAsync(c => c.Id == user.LastUsedCompanyId, cancellationToken);
        }
        
        // Update last login time
        user.LastLoginTime = DateTime.Now;
        await UserManager.UpdateAsync(user);

        // Generate tokens
        var accessToken = TokenService.GenerateAccessToken(user, company);
        var refreshToken = await TokenService.GenerateRefreshTokenAsync(user.Id, company?.Id);

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
            AccessToken = accessToken,
            UserId = user.Id,
            UserType = user.UserType
        }, cancellation: cancellationToken);
    }
}