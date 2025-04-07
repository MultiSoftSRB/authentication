using Microsoft.AspNetCore.Identity;
using MultiSoftSRB.Entities.Main;
using MultiSoftSRB.Entities.Main.Enums;
using MultiSoftSRB.Services;

namespace MultiSoftSRB.Features.Auth.LoginSuperadmin;

sealed class Endpoint : Endpoint<Request, Response>
{
    public UserManager<User> UserManager { get; set; }
    public SignInManager<User> SignInManager { get; set; }
    public TokenService TokenService { get; set; }
    
    public override void Configure()
    {
        Post("auth/login-superadmin");
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

        if (user.UserType != UserType.SuperAdmin || user.UserType != UserType.Consultant)
        {
            AddError("Only Super Admin users can login via this endpoint");
            await SendErrorsAsync(StatusCodes.Status403Forbidden, cancellationToken);
            return;
        }
        
        // Update last login time
        user.LastLoginTime = DateTime.Now;
        await UserManager.UpdateAsync(user);
        
        // Generate tokens
        var tokens = await TokenService.GenerateTokensAsync(user);
        
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

        var response = new Response
        {
            AccessToken = tokens.AccessToken,
            UserDetails = new()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.UserName
            }
        };
        
        await SendOkAsync(response, cancellationToken);
    }
}