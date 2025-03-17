using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Auth.Users.GetUser;

sealed class Endpoint : EndpointWithoutRequest<Response>
{
    public MainDbContext MainDbContext { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    
    public override void Configure()
    {
        Get("auth/users/{id}");
        Permissions(ResourcePermissions.UserRead);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var companyId = CompanyProvider.GetCompanyId();
        var userId = Route<long>("id");
        
        var user = await MainDbContext.Users
            .Where(u => u.Id == userId && u.UserCompanies.Any(uc => uc.CompanyId == companyId))
            .Include(u => u.UserRoles.Where(ur => ur.CompanyId == companyId))
            .Select(u => new Response
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                UserType = u.UserType,
                RoleId = u.UserRoles
                    .Select(ur => ur.RoleId)
                    .FirstOrDefault(),
                RoleName = u.UserRoles
                    .Select(ur => ur.Role.Name)
                    .FirstOrDefault() ?? "No Role",
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        await SendOkAsync(user, cancellationToken);
    }
}