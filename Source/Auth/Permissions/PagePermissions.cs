using System.Reflection;

namespace MultiSoftSRB.Auth.Permissions;

public static class PagePermissions
{
    // Company management
    public const string CompaniesView = "companies.view-page";
    public const string CompaniesCreate = "companies.create-page";
    public const string CompaniesEdit = "companies.edit-page";
    
    // User management
    public const string UsersView = "users.view-page";
    public const string UsersCreate = "users.create-page";
    public const string UsersEdit = "users.edit-page";
    
    // Role management
    public const string RolesView = "roles.view-page";
    public const string RolesCreate = "roles.create-page";
    public const string RolesEdit = "roles.edit-page";
    
    // Client management
    public const string ClientsView = "clients.view-page";
    public const string ClientsCreate = "clients.create-page";
    public const string ClientsEdit = "clients.edit-page";
    
    // Invoice management
    public const string InvoicesView = "invoices.view-page";
    public const string InvoicesCreate = "invoices.create-page";
    public const string InvoicesEdit = "invoices.edit-page";
    
    // Agency management
    public const string AgencyView = "agency.view-page";
    public const string AgencyClients = "agency.clients-page";
    
    // Feature management
    public const string FeaturesView = "features.view-page";
    public const string FeaturesEdit = "features.edit-page";
    
    // API key management
    public const string ApiKeysView = "apikeys.view-page";
    public const string ApiKeysCreate = "apikeys.create-page";
    public const string ApiKeysEdit = "apikeys.edit-page";
    
    // Helper method to get all permissions
    public static IEnumerable<string> GetAll()
    {
        return typeof(PagePermissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(x => (string)x.GetRawConstantValue()!)
            .ToList();
    }
}