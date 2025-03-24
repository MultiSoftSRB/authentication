namespace MultiSoftSRB.Auth.ApiKey;

public class ApiKeyInfo
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string[] Permissions { get; set; } = [];
}