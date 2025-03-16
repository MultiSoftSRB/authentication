using System.Collections.ObjectModel;

namespace MultiSoftSRB.Auth.Permissions;

public static class PermissionMapper
{
    // Define page to resource permission mappings
    private static readonly IReadOnlyDictionary<string, string[]> _pageToResourceMap = 
        new ReadOnlyDictionary<string, string[]>(new Dictionary<string, string[]>
        {
            // Company management
            [PagePermissions.CompaniesView] =
            [
                ResourcePermissions.CompanyList, 
                ResourcePermissions.CompanyRead
            ],
            
            [PagePermissions.CompaniesCreate] =
            [
                ResourcePermissions.CompanyCreate
            ],
            
            [PagePermissions.CompaniesEdit] =
            [
                ResourcePermissions.CompanyUpdate
            ],
            
            // User management
            [PagePermissions.UsersView] =
            [
                ResourcePermissions.UserList, 
                ResourcePermissions.UserRead
            ],
            
            [PagePermissions.UsersCreate] =
            [
                ResourcePermissions.UserCreate
            ],
            
            [PagePermissions.UsersEdit] =
            [
                ResourcePermissions.UserUpdate
            ],
            
            // Role management
            [PagePermissions.RolesView] =
            [
                ResourcePermissions.RoleList, 
                ResourcePermissions.RoleRead
            ],
            
            [PagePermissions.RolesCreate] =
            [
                ResourcePermissions.RoleCreate
            ],
            
            [PagePermissions.RolesEdit] =
            [
                ResourcePermissions.RoleUpdate
            ],
            
            // Client management
            [PagePermissions.ClientsView] =
            [
                ResourcePermissions.ClientList, 
                ResourcePermissions.ClientRead
            ],
            
            [PagePermissions.ClientsCreate] =
            [
                ResourcePermissions.ClientCreate
            ],
            
            [PagePermissions.ClientsEdit] =
            [
                ResourcePermissions.ClientUpdate
            ],
            
            // Invoice management
            [PagePermissions.InvoicesView] =
            [
                ResourcePermissions.InvoiceList, 
                ResourcePermissions.InvoiceRead
            ],
            
            [PagePermissions.InvoicesCreate] =
            [
                ResourcePermissions.InvoiceCreate, 
                ResourcePermissions.ClientList  // Need to list clients when creating invoice
            ],
            
            [PagePermissions.InvoicesEdit] =
            [
                ResourcePermissions.InvoiceUpdate
            ],
            
            // Agency management
            [PagePermissions.AgencyView] =
            [
                ResourcePermissions.AgencyList, 
                ResourcePermissions.AgencyRead
            ],
            
            [PagePermissions.AgencyClients] =
            [
                ResourcePermissions.AgencyManage, 
                ResourcePermissions.CompanyList
            ],
            
            // Feature management
            [PagePermissions.FeaturesView] =
            [
                ResourcePermissions.FeatureList, 
                ResourcePermissions.FeatureRead
            ],
            
            [PagePermissions.FeaturesEdit] =
            [
                ResourcePermissions.FeatureUpdate
            ],
            
            // API key management
            [PagePermissions.ApiKeysView] =
            [
                ResourcePermissions.ApiKeyList, 
                ResourcePermissions.ApiKeyRead
            ],
            
            [PagePermissions.ApiKeysCreate] =
            [
                ResourcePermissions.ApiKeyCreate
            ],
            
            [PagePermissions.ApiKeysEdit] =
            [
                ResourcePermissions.ApiKeyUpdate
            ]
        });
    
    public static string[] MapToResourcePermissions(IEnumerable<string> pagePermissions)
    {
        var resourcePermissions = new HashSet<string>();
        
        foreach (var pagePermission in pagePermissions)
        {
            if (_pageToResourceMap.TryGetValue(pagePermission, out var resources))
            {
                foreach (var resource in resources)
                {
                    resourcePermissions.Add(resource);
                }
            }
        }
        
        return resourcePermissions.ToArray();
    }
    
    public static string[] GetResourcePermissionsForPage(string pagePermission)
    {
        return _pageToResourceMap.TryGetValue(pagePermission, out var resources) ? resources : [];
    }
    
    public static IReadOnlyDictionary<string, string[]> GetAllMappings()
    {
        return _pageToResourceMap;
    }
}