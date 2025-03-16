namespace MultiSoftSRB.Entities.Main;

public class Role : BaseEntity
{
    public long CompanyId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    // Navigation properties
    public virtual Company Company { get; set; } = null!;
    public virtual ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}