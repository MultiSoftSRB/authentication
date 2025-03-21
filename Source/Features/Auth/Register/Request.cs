using FluentValidation;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Features.Auth.Register;

sealed class Request
{
    // User info
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string UserNameWithoutCompanyCode { get; set; } = null!;
        
    // Company info
    public string CompanyName { get; set; } = null!;
    public string CompanyCode { get; set; } = null!;
    public DatabaseType CompanyType { get; set; }
    
    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.UserNameWithoutCompanyCode)
                .NotEmpty().WithMessage("Username is required");
                
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
            
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");
            
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");
            
            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company name is required")
                .MaximumLength(100).WithMessage("Company name cannot exceed 100 characters");
            
            RuleFor(x => x.CompanyCode)
                .NotEmpty().WithMessage("Company code is required")
                .MaximumLength(20).WithMessage("Company code cannot exceed 20 characters")
                .Matches("^[a-zA-Z0-9-_]+$").WithMessage("Company code can only contain letters, numbers, hyphens, and underscores");
        }
    }
}