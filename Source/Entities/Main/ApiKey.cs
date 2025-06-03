namespace MultiSoftSRB.Entities.Main;

public class ApiKey : BaseEntity
{
    public short CompanyId { get; set; }
    public string KeyHash { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public long CreatedBy { get; set; }
        
    // Navigation properties
    public virtual Company Company { get; set; } = null!;
    public virtual User CreatedByUser { get; set; } = null!;
    public virtual ICollection<ApiKeyPermission> Permissions { get; set; } = new List<ApiKeyPermission>();
}