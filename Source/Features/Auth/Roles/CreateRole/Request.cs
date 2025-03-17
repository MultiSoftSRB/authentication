using FluentValidation;

namespace MultiSoftSRB.Features.Auth.Roles.CreateRole;

sealed class Request
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = [];
    
    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100)
                .WithMessage("Role name is required and must be less than 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .When(x => x.Description != null)
                .WithMessage("Description must be less than 500 characters");

            RuleFor(x => x.Permissions)
                .NotEmpty()
                .WithMessage("At least one permission must be assigned to the role");
        }
    }
}