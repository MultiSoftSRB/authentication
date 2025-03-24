namespace MultiSoftSRB.Entities.Audit;

public class AuditLog
{
    public long Id { get; set; }
    public string EntityName { get; set; }
    public ActionType ActionType { get; set; }
    public DateTime Timestamp { get; set; }
    
    // User identification - either user or API key
    public long? UserId { get; set; }
    public string? UserName { get; set; }
    public long? ApiKeyId { get; set; }
    public bool IsApiKeyAuth { get; set; }
    
    public long? CompanyId { get; set; }
    public string EntityId { get; set; }
    public string Endpoint { get; set; }
    public string OldValues { get; set; }
    public string NewValues { get; set; }
    public string ChangedProperties { get; set; }
    public string ContextType { get; set; }
}