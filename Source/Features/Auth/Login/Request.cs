using FluentValidation;

namespace MultiSoftSRB.Features.Auth.Login;

sealed class Request
{
    public string Username { get; set; }
    public string Password { get; set; }
    
    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}