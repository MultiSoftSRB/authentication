using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Features.Auth.Login;

sealed class Response
{
    public string AccessToken { get; set; } = null!;
    public long UserId { get; set; }
    public UserType UserType { get; set; }
}