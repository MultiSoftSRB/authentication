using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Entities.Main;

public class User : IdentityUser<long>
{
    
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public UserType UserType { get; set; }
    public short? LastUsedCompanyId { get; set; }
    public DateTime LastLoginTime { get; set; }
    
    public int? GeoSettlementId { get; set; }

    [MaxLength(50)]
    public string TimeZone { get; set; } = "UTC";
    
    
    [MaxLength(10)]
    public string Locale { get; set; } = "sr-RS";
    
    
    
    
    
    
    
    // Navigation properties
    public virtual ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}