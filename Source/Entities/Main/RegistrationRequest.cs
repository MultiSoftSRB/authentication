using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Entities.Main;

public class RegistrationRequest : BaseEntity
{
    public DateTime Timestamp { get; set; }
    
    // User info
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string UserNameWithoutCompanyCode { get; set; } = null!;
        
    // Company info
    public string CompanyName { get; set; } = null!;
    public string CompanyCode { get; set; } = null!;
    public DatabaseType CompanyType { get; set; }
}