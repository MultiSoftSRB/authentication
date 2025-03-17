using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Features.Auth.Users.CreateUser;

sealed class Endpoint : Endpoint<Request, Response>
{
    public MainDbContext MainDbContext { get; set; }
    public UserManager<User> UserManager { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    
    public override void Configure()
    {
        Post("auth/users");
        Permissions(ResourcePermissions.UserCreate);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var companyId = CompanyProvider.GetCompanyId();

        // Verify the role belongs to the company
        var roleExists = await MainDbContext.Roles
            .Where(r => r.CompanyId == companyId && r.Id == request.RoleId)
            .AnyAsync(cancellationToken);

        if (!roleExists)
            ThrowError("Invalid role");
        
        var companyCode = await MainDbContext.Companies
            .Where(c => c.Id == companyId)
            .Select(c => c.Code)
            .SingleOrDefaultAsync(cancellationToken);
        
        // Check if user with this email already exists
        var username = $"{companyCode}.{request.Username}";
        var usernameExists = await UserManager.FindByNameAsync(username);
        if (usernameExists != null)
        {
            AddError(r => r.Email, "A user with this username already exists");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }

        // Create the new user
        var user = new User
        {
            UserName = username,
            NormalizedUserName = username.ToUpper(),
            Email = request.Email,
            NormalizedEmail = request.Email.ToUpper(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserType = UserType.TenantUser
        };

        // Create the user with password
        var result = await UserManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                AddError("User", error.Description);
            }
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }

        // Associate user with company
        var userCompany = new UserCompany
        {
            UserId = user.Id,
            CompanyId = companyId,
            IsPrimary = true
        };
        await MainDbContext.UserCompanies.AddAsync(userCompany, cancellationToken);

        // Assign role to user
        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = request.RoleId,
            CompanyId = companyId
        };
        await MainDbContext.UserRoles.AddAsync(userRole, cancellationToken);

        // Save all changes
        await MainDbContext.SaveChangesAsync(cancellationToken);

        var response = new Response
        {
            Id = user.Id
        };

        await SendOkAsync(response, cancellationToken);
    }
}