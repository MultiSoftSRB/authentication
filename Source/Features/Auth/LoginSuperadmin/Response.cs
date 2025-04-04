namespace MultiSoftSRB.Features.Auth.LoginSuperadmin;

sealed class Response
{
    public string AccessToken { get; set; }
    public User UserDetails { get; set; }

    internal sealed class User
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}