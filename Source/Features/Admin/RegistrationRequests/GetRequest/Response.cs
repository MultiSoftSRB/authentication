using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Features.Admin.RegistrationRequests.GetRequest;

sealed class Response
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    
    // User info
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserNameWithoutCompanyCode { get; set; }
        
    // Company info
    public string CompanyName { get; set; }
    public string CompanyCode { get; set; }
    public DatabaseType CompanyType { get; set; }
}