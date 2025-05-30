using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Entities.Main;

public class Company : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DatabaseType DatabaseType { get; set; }
    public long? LicenseId { get; set; }
    public short LicenseCount { get; set; } = 1;
    
    public bool IsAgency { get; set; }
        
    // Navigation properties
    public virtual License? License { get; set; }
    public virtual ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    public virtual ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
}