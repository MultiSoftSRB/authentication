using Microsoft.AspNetCore.Identity;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Entities.Main;

public class User : IdentityUser<long>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public UserType UserType { get; set; }
    public long? LastUsedCompanyId { get; set; }
    public DateTime LastLoginTime { get; set; }
    
    // Navigation properties
    public virtual ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}