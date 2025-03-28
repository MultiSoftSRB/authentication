using System.Reflection;

namespace MultiSoftSRB.Auth.Permissions;

public static class ResourcePermissions
{
    // Audit
    public const string AuditRead = "audit.read";
    
    // Company management
    public const string CompanyList = "company.list";
    public const string CompanyRead = "company.read";
    public const string CompanyCreate = "company.create";
    public const string CompanyUpdate = "company.update";
    public const string CompanyDelete = "company.delete";
    
    // User management
    public const string UserList = "user.list";
    public const string UserRead = "user.read";
    public const string UserCreate = "user.create";
    public const string UserUpdate = "user.update";
    public const string UserDelete = "user.delete";
    
    // Role management
    public const string RoleList = "role.list";
    public const string RoleRead = "role.read";
    public const string RoleCreate = "role.create";
    public const string RoleUpdate = "role.update";
    public const string RoleDelete = "role.delete";
    
    // Client management
    public const string ClientList = "client.list";
    public const string ClientRead = "client.read";
    public const string ClientCreate = "client.create";
    public const string ClientUpdate = "client.update";
    public const string ClientDelete = "client.delete";
    
    // Invoice management
    public const string InvoiceList = "invoice.list";
    public const string InvoiceRead = "invoice.read";
    public const string InvoiceCreate = "invoice.create";
    public const string InvoiceUpdate = "invoice.update";
    public const string InvoiceDelete = "invoice.delete";
    
    // Agency management
    public const string AgencyList = "agency.list";
    public const string AgencyRead = "agency.read";
    public const string AgencyManage = "agency.manage";
    
    // Feature management
    public const string FeatureList = "feature.list";
    public const string FeatureRead = "feature.read";
    public const string FeatureUpdate = "feature.update";
    
    // API key management
    public const string ApiKeyList = "apikey.list";
    public const string ApiKeyRead = "apikey.read";
    public const string ApiKeyCreate = "apikey.create";
    public const string ApiKeyUpdate = "apikey.update";
    public const string ApiKeyDelete = "apikey.delete";
    
    // Helper method to get all permissions
    public static IEnumerable<string> GetAll()
    {
        return typeof(ResourcePermissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(x => (string)x.GetRawConstantValue()!)
            .ToList();
    }
}