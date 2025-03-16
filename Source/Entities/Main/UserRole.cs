namespace MultiSoftSRB.Entities.Main;

public class UserRole
{
    public long UserId { get; set; }
    public long RoleId { get; set; }
    public long CompanyId { get; set; }
        
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
    public virtual Company Company { get; set; } = null!;
}