using FluentValidation;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Features.Agency.Clients.CreateCompany;

sealed class Request
{
    public string CompanyName { get; set; } = null!;
    public string CompanyCode { get; set; } = null!;
    public DatabaseType CompanyType { get; set; }
    
    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
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