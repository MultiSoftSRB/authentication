namespace MultiSoftSRB.Features.Auth.Roles.GetRoles;

sealed class Response
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
}