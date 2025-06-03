using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Entities.Main;

public class UserCompany
{
    public long UserId { get; set; }
    public short CompanyId { get; set; }
    public bool IsPrimary { get; set; }
    public AccessType AccessType { get; set; } = AccessType.Direct;
    
    public short? AgencyCompanyId { get; set; }
        
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Company Company { get; set; } = null!;
    public virtual Company? AgencyCompany { get; set; }
}