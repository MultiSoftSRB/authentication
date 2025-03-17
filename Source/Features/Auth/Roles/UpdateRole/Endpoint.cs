using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Features.Auth.Roles.UpdateRole;

sealed class Endpoint : Endpoint<Request>
{
    public MainDbContext MainDbContext { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    
    public override void Configure()
    {
        Put("auth/roles/{id}");
        Permissions(ResourcePermissions.RoleUpdate);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        // Validate permissions
        var allValidPagePermissions = PagePermissions.GetAll().ToHashSet();
        var invalidPermissions = request.Permissions
            .Where(p => !allValidPagePermissions.Contains(p))
            .ToList();

        if (invalidPermissions.Count != 0)
            ThrowError($"Invalid permissions: {string.Join(", ", invalidPermissions)}");
        
        var companyId = CompanyProvider.GetCompanyId();
        var roleId = Route<int>("id");
        var role = await MainDbContext.Roles
            .Where(r => r.CompanyId == companyId && r.Id == roleId)
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(cancellationToken);

        if (role == null)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }
        
        // Get existing permission codes
        var existingPermissionCodes = role.Permissions
            .Select(p => p.PagePermissionCode)
            .ToHashSet();

        // Determine permissions to add and remove
        var permissionsToAdd = request.Permissions
            .Where(p => !existingPermissionCodes.Contains(p))
            .ToList();

        var permissionsToRemove = role.Permissions
            .Where(p => !request.Permissions.Contains(p.PagePermissionCode))
            .ToList();

        // Remove permissions that are no longer needed
        if (permissionsToRemove.Count != 0)
        {
            MainDbContext.RemoveRange(permissionsToRemove);
        }
        
        // Add new permissions
        foreach (var permission in permissionsToAdd)
        {
            role.Permissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PagePermissionCode = permission
            });
        }

        await MainDbContext.SaveChangesAsync(cancellationToken);

        await SendOkAsync(cancellationToken);
    }
}