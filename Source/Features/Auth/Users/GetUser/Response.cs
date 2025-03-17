using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Features.Auth.Users.GetUser;

sealed class Response
{
    public long Id { get; set; }
    public string? Email { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public long RoleId { get; set; }
    public string RoleName { get; set; } = null!;
    public UserType UserType { get; set; }
}