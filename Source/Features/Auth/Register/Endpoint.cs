using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth.Permissions;
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
        
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        try
        {
            // Create company
            var company = new Company
            {
                Name = request.CompanyName,
                Code = request.CompanyCode,
                DatabaseType = request.CompanyType
            };
            
            MainDbContext.Companies.Add(company);
            await MainDbContext.SaveChangesAsync(cancellationToken);
            
            // Create user
            var user = new User
            {
                UserName = $"{company.Code}.admin",
                NormalizedUserName = $"{company.Code}.admin".ToUpper(),
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
            
            MainDbContext.UserCompanies.Add(userCompany);
            
            // Create default roles
            var roles = await CreateDefaultRolesAsync(company.Id, cancellationToken);
            
            // Assign admin role to user
            var adminRole = roles.FirstOrDefault()!;
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = adminRole.Id,
                CompanyId = company.Id
            };
            
            MainDbContext.UserRoles.Add(userRole);
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
            transaction.Complete();
            
            await SendOkAsync(new Response
            {
                UserId = user.Id,
                CompanyId = company.Id,
                AccessToken = accessToken,
            }, cancellation: cancellationToken);
        }
        catch (Exception)
        {
            AddError("An error occurred during registration");
            await SendErrorsAsync(StatusCodes.Status500InternalServerError, cancellationToken);
        }
    }
    
    private async Task<List<Role>> CreateDefaultRolesAsync(long companyId, CancellationToken cancellationToken)
    {
        var roles = new List<Role>
        {
            new Role
            {
                CompanyId = companyId,
                Name = "Administrator",
                Description = "Full access to all features",
                Permissions = PagePermissions
                              .GetAll()
                              .Select(rp => new RolePermission { PagePermissionCode = rp })
                              .ToList()
            },
            new Role
            {
                CompanyId = companyId,
                Name = "Accountant",
                Description = "Access to accounting features",
                Permissions = new List<RolePermission>
                {
                    // Add accounting-related permissions
                    new RolePermission { PagePermissionCode = PagePermissions.ClientsView },
                    new RolePermission { PagePermissionCode = PagePermissions.ClientsCreate },
                    new RolePermission { PagePermissionCode = PagePermissions.ClientsEdit },
                    new RolePermission { PagePermissionCode = PagePermissions.InvoicesView },
                    new RolePermission { PagePermissionCode = PagePermissions.InvoicesCreate },
                    new RolePermission { PagePermissionCode = PagePermissions.InvoicesEdit }
                }
            }
        };
        
        await MainDbContext.Roles.AddRangeAsync(roles, cancellationToken);
        await MainDbContext.SaveChangesAsync(cancellationToken);
        
        return roles;
    }
}