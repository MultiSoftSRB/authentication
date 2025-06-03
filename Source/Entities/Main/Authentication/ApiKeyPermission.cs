namespace MultiSoftSRB.Entities.Main;

public class ApiKeyPermission
{
    public long ApiKeyId { get; set; }
    public string ResourcePermissionCode { get; set; } = null!;
        
    // Navigation property
    public virtual ApiKey ApiKey { get; set; } = null!;
}