using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Features.Auth.GetMe;

sealed class Response
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public UserType UserType { get; set; }
    public string[] PagePermissions { get; set; }
    
    public  List<Company> UserCompanies { get; set; } = new();
    
    internal sealed class Company
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
    
    
}