using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Auth.Roles.GetRole;

sealed class Endpoint : EndpointWithoutRequest<Response>
{
    public MainDbContext MainDbContext { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    
    public override void Configure()
    {
        Get("auth/roles/{id}");
        Permissions(ResourcePermissions.RoleRead);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var roleId = Route<int>("id");
        var companyId = CompanyProvider.GetCompanyId();
        
        var role = await MainDbContext.Roles
            .Where(r => r.CompanyId == companyId && r.Id == roleId)
            .AsNoTracking()
            .Select(r => new Response
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Permissions = r.Permissions.Select(p => p.PagePermissionCode).ToList(),
                Users = r.UserRoles.Select(u => u.User.FirstName + " " + u.User.LastName).ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (role == null)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        await SendOkAsync(role, cancellationToken);
    }
}