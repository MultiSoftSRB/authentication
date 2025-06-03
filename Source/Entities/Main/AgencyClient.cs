namespace MultiSoftSRB.Entities.Main;

public class AgencyClient
{
    public short AgencyCompanyId { get; set; }
    public short ClientCompanyId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
    // Navigation properties
    public virtual Company AgencyCompany { get; set; } = null!;
    public virtual Company ClientCompany { get; set; } = null!;
}