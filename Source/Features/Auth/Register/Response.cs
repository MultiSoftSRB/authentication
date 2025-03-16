namespace MultiSoftSRB.Features.Auth.Register;

sealed class Response
{
    public long UserId { get; set; }
    public long CompanyId { get; set; }
    public string AccessToken { get; set; } = null!;
}