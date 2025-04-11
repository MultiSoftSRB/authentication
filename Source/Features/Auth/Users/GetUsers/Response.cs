namespace MultiSoftSRB.Features.Auth.Users.GetUsers;

sealed class Response
{
    public long Id { get; set; }

    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Role { get; set; } = null!;

    public string FullName { get; set; } = null;
}