namespace MultiSoftSRB.Features.Admin.RegistrationRequests.GetRequests;

sealed class Response
{
    public long Id { get; set; }
    public string CompanyName { get; set; }
    public string UserFullName { get; set; }
    public string Email { get; set; }
    public DateTime Timestamp { get; set; }
}