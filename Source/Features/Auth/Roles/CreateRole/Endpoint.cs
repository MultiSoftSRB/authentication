using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Features.Auth.Roles.CreateRole;

sealed class Endpoint : Endpoint<Request, Response>
{
    public MainDbContext MainDbContext { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    
    public override void Configure()
    {
        Post("auth/roles");
        Permissions(ResourcePermissions.RoleCreate);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var companyId = CompanyProvider.GetCompanyId();
        
        // Validate permissions
        var allValidPagePermissions = PagePermissions.GetAll().ToHashSet();
        var invalidPermissions = request.Permissions
            .Where(p => !allValidPagePermissions.Contains(p))
            .ToList();

        if (invalidPermissions.Count != 0)
            ThrowError($"Invalid permissions: {string.Join(", ", invalidPermissions)}");

        // Create new role
        var role = new Role
        {
            CompanyId = companyId,
            Name = request.Name,
            Description = request.Description
        };

        // Add permissions
        foreach (var permission in request.Permissions)
        {
            role.Permissions.Add(new RolePermission
            {
                PagePermissionCode = permission
            });
        }

        await MainDbContext.Roles.AddAsync(role, cancellationToken);
        await MainDbContext.SaveChangesAsync(cancellationToken);

        var response = new Response
        {
            Id = role.Id,
            Name = role.Name
        };

        await SendOkAsync(response, cancellationToken);
    }
}