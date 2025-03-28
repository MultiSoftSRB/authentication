namespace MultiSoftSRB.Entities.Audit;

public abstract class AuditLog
{
    public long Id { get; set; }
    public AuditActionType ActionType { get; set; }
    public DateTime Timestamp { get; set; }
    
    // User identification - either user or API key
    public long? UserId { get; set; }
    public string? UserName { get; set; }
    public long? ApiKeyId { get; set; }
    
    public long? CompanyId { get; set; }
    public string EntityId { get; set; }
    public string Endpoint { get; set; }
    public string ChangedProperties { get; set; }
}

// Empty derived classes just for table separation from Main entities
public class DefaultAuditLog : AuditLog { }
public class CompanyAuditLog : AuditLog { }
public class UserAuditLog : AuditLog { }
public class RoleAuditLog : AuditLog { }
public class RolePermissionAuditLog : AuditLog { }
public class UserCompanyAuditLog : AuditLog { }
public class UserRoleAuditLog : AuditLog { }
public class ApiKeyAuditLog : AuditLog { }
public class ApiKeyPermissionAuditLog : AuditLog { }
public class AgencyClientAuditLog : AuditLog { }


// Empty derived classes just for table separation from Company entities
public class ArticleAuditLog : AuditLog { }