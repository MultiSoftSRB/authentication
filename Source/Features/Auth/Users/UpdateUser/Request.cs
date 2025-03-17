using FluentValidation;

namespace MultiSoftSRB.Features.Auth.Users.UpdateUser;

sealed class Request
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public long RoleId { get; set; }
    
    internal sealed class UpdateCompanyUserValidator : Validator<Request>
    {
        public UpdateCompanyUserValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

            RuleFor(x => x.RoleId)
                .GreaterThan(0).WithMessage("A valid role must be selected");
        }
    }
}