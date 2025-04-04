using System.Reflection;

namespace MultiSoftSRB.Auth.Permissions;

public static class FeaturePermissions
{
    public const string Invoicing = "feature.invoicing";
    public const string Payroll = "feature.payroll";
    public const string DocumentManagementSystem = "feature.document-management-system";
    
    // Helper method to get all permissions
    public static IEnumerable<string> GetAll()
    {
        return typeof(FeaturePermissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(string))
            .Select(x => (string)x.GetRawConstantValue()!)
            .ToList();
    }
}