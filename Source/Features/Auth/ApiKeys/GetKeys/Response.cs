namespace MultiSoftSRB.Features.Auth.ApiKeys.GetKeys;

sealed class Response
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string CreatedBy { get; set; } = null!;
    public string Status { get; set; } = null!;
    public List<string> Permissions { get; set; } = [];
}