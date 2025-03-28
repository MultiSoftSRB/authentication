using MultiSoftSRB.Entities.Audit;

namespace MultiSoftSRB.Contracts.Common;

public class AuditLogResponse
{
    public long Id { get; set; }
    public string EntityId { get; set; }
    public AuditActionType ActionType { get; set; }
    public DateTime Timestamp { get; set; }
    public long? CompanyId { get; set; }
    public long? UserId { get; set; }
    public string? UserName { get; set; }
    public long? ApiKeyId { get; set; }
    public string Endpoint { get; set; }
    public List<PropertyChange> ChangedProperties { get; set; }
}