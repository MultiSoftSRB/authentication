namespace MultiSoftSRB.Auth.ApiKey;

public class ApiKeyValidationResult
{
    public bool IsValid { get; set; }
    public string? FailureReason { get; set; }
    public long? ApiKeyId { get; set; }
    public long CompanyId { get; set; }
    public long? UserId { get; set; }
    public string[] Permissions { get; set; } = Array.Empty<string>();
}