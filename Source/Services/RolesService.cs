using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Services;

public class RolesService
{
    public List<Role> GenerateDefaultRoles(long companyId)
    {
        return
        [
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
                    new() { PagePermissionCode = PagePermissions.ClientsView },
                    new() { PagePermissionCode = PagePermissions.ClientsCreate },
                    new() { PagePermissionCode = PagePermissions.ClientsEdit },
                    new() { PagePermissionCode = PagePermissions.InvoicesView },
                    new() { PagePermissionCode = PagePermissions.InvoicesCreate },
                    new() { PagePermissionCode = PagePermissions.InvoicesEdit }
                }
            }
        ];
    }
}