namespace MultiSoftSRB.Entities.Main;

public class UserCompany
{
    public long UserId { get; set; }
    public long CompanyId { get; set; }
    public bool IsPrimary { get; set; }
        
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Company Company { get; set; } = null!;
}