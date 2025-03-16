namespace MultiSoftSRB.Entities.Main;

public class RolePermission
{
    public long RoleId { get; set; }
    public string PagePermissionCode { get; set; } = null!;
        
    // Navigation property
    public virtual Role Role { get; set; } = null!;
}