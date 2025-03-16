namespace MultiSoftSRB.Features.Auth.GetMe;

sealed class Response
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string[] PagePermissions { get; set; }
}