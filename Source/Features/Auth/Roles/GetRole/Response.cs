namespace MultiSoftSRB.Features.Auth.Roles.GetRole;

sealed class Response
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = [];
    public List<string> Users { get; set; }
}