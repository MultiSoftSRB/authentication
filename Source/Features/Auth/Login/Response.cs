using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Features.Auth.Login;

sealed class Response
{
    public string AccessToken { get; set; }
    public User UserDetails { get; set; }
    public List<Company> Companies { get; set; }

    internal sealed class User
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public UserType UserType { get; set; }
    }

    internal sealed class Company
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
}