using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Auth.Roles.DeleteRole;

sealed class Endpoint : EndpointWithoutRequest
{
    public MainDbContext MainDbContext { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    
    public override void Configure()
    {
        Delete("/auth/roles/{id}");
        Permissions(ResourcePermissions.RoleDelete);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var roleId = Route<int>("id");
        var companyId = CompanyProvider.GetCompanyId();
        
        // Check if role exists
        var role = await MainDbContext.Roles
            .Where(r => r.CompanyId == companyId && r.Id == roleId)
            .FirstOrDefaultAsync(cancellationToken);

        if (role == null)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }
        
        // Check if role is in use
        if (await MainDbContext.UserRoles.AnyAsync(ur => ur.RoleId == roleId, cancellationToken))
            ThrowError("Cannot delete a role that is assigned to users");

        // Remove role permissions
        MainDbContext.Remove(role);
        await MainDbContext.SaveChangesAsync(cancellationToken);
        
        await SendNoContentAsync(cancellationToken);
    }
}