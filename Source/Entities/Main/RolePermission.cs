namespace MultiSoftSRB.Entities.Main;

public class RolePermission
{
    public int RoleId { get; set; }
    public string PagePermissionCode { get; set; } = null!;
        
    // Navigation property
    public virtual Role Role { get; set; } = null!;
}