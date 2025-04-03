namespace MultiSoftSRB.Auth.Licensing;

public class UserSessionInfo
{
    public long UserId { get; set; }
    public long? CompanyId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivity { get; set; }
}