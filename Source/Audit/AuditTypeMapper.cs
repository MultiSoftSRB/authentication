using MultiSoftSRB.Entities.Audit;
using MultiSoftSRB.Entities.Company;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Audit;

public static class AuditTypeMapper
{
    // Static map from entity types to audit types
    private static readonly Dictionary<Type, Type> EntityToAuditTypeMap = new();
    
    static AuditTypeMapper()
    {
        // Main context entities
        EntityToAuditTypeMap[typeof(Company)] = typeof(CompanyAuditLog);
        EntityToAuditTypeMap[typeof(User)] = typeof(UserAuditLog);
        EntityToAuditTypeMap[typeof(Role)] = typeof(RoleAuditLog);
        EntityToAuditTypeMap[typeof(RolePermission)] = typeof(RolePermissionAuditLog);
        EntityToAuditTypeMap[typeof(UserCompany)] = typeof(UserCompanyAuditLog);
        EntityToAuditTypeMap[typeof(UserRole)] = typeof(UserRoleAuditLog);
        EntityToAuditTypeMap[typeof(ApiKey)] = typeof(ApiKeyAuditLog);
        EntityToAuditTypeMap[typeof(ApiKeyPermission)] = typeof(ApiKeyPermissionAuditLog);
        EntityToAuditTypeMap[typeof(AgencyClient)] = typeof(AgencyClientAuditLog);
        
        // Company context entities
        EntityToAuditTypeMap[typeof(Article)] = typeof(ArticleAuditLog);
    }
    
    // Get the audit type for a given entity type
    public static Type GetAuditTypeForEntity(Type entityType)
    {
        return EntityToAuditTypeMap.TryGetValue(entityType, out var auditType) 
            ? auditType 
            : typeof(DefaultAuditLog);
    }
    
    // Create a new audit log instance of the appropriate type
    public static AuditLog CreateAuditLogFor(Type entityType)
    {
        var auditType = GetAuditTypeForEntity(entityType);
        return (AuditLog)Activator.CreateInstance(auditType);
    }
}