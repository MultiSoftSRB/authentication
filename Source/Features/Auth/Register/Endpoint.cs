using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;
using MultiSoftSRB.Entities.Main.Enums;
using MultiSoftSRB.Services;

namespace MultiSoftSRB.Features.Auth.Register;

sealed class Endpoint : Endpoint<Request, Response>
{
    public UserManager<User> UserManager { get; set; }
    public MainDbContext MainDbContext { get; set; }
    public TokenService TokenService { get; set; }
    public RolesService RolesService { get; set; }
    
    public override void Configure()
    {
        Post("auth/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingUser = await UserManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            AddError("Email already in use");
            await SendErrorsAsync(StatusCodes.Status409Conflict, cancellationToken);
            return;
        }
        
        // Check if company code already exists
        var existingCompany = await MainDbContext.Companies.AnyAsync(c => c.Code == request.CompanyCode, cancellationToken);
        if (existingCompany)
        {
            AddError("Company code already in use");
            await SendErrorsAsync(StatusCodes.Status409Conflict, cancellationToken);
            return;
        }
        
        await using var transaction = await MainDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Create company
            var company = new Company
            {
                Name = request.CompanyName,
                Code = request.CompanyCode,
                DatabaseType = request.CompanyType
            };
            
            await MainDbContext.Companies.AddAsync(company, cancellationToken);
            await MainDbContext.SaveChangesAsync(cancellationToken);
            
            // Create user
            var user = new User
            {
                UserName = $"{company.Code}.{request.UserNameWithoutCompanyCode}",
                NormalizedUserName = $"{company.Code}.{request.UserNameWithoutCompanyCode}".ToUpper(),
                Email = request.Email,
                NormalizedEmail = request.Email.ToUpper(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserType = UserType.TenantAdmin,
                LastUsedCompanyId = company.Id
            };
            
            var createUserResult = await UserManager.CreateAsync(user, request.Password);
            if (!createUserResult.Succeeded)
            {
                var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                
                AddError(errors);
                await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
                return;
            }
            
            // Associate user with company
            var userCompany = new UserCompany
            {
                UserId = user.Id,
                CompanyId = company.Id,
                IsPrimary = true
            };
            
            await MainDbContext.UserCompanies.AddAsync(userCompany, cancellationToken);
            
            // Create default roles
            var roles = RolesService.GenerateDefaultRoles(company.Id);
            await MainDbContext.Roles.AddRangeAsync(roles, cancellationToken);
            await MainDbContext.SaveChangesAsync(cancellationToken);
            
            // Assign admin role to user
            var adminRole = roles.FirstOrDefault()!;
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = adminRole.Id,
                CompanyId = company.Id
            };
            
            await MainDbContext.UserRoles.AddAsync(userRole, cancellationToken);
            await MainDbContext.SaveChangesAsync(cancellationToken);
            
            // Generate tokens
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
            
            // After all the changes, now we commit the transaction
            await transaction.CommitAsync(cancellationToken);
            
            await SendOkAsync(new Response
            {
                UserId = user.Id,
                CompanyId = company.Id,
                AccessToken = accessToken,
            }, cancellation: cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            AddError("An error occurred during registration");
            await SendErrorsAsync(StatusCodes.Status500InternalServerError, cancellationToken);
        }
    }
}