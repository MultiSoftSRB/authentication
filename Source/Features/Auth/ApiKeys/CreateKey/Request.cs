using FluentValidation;

namespace MultiSoftSRB.Features.Auth.ApiKeys.CreateKey;

sealed class Request { 
    public short CompanyId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime? ExpiresAt { get; set; }
    public List<string> Permissions { get; set; } = [];
    
    internal sealed class CreateApiKeyValidator : Validator<Request>
    {
        public CreateApiKeyValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("API key name is required")
                .MaximumLength(100).WithMessage("API key name cannot exceed 100 characters");
            
            RuleFor(x => x.ExpiresAt)
                .Must(x => x == null || x > DateTime.Now)
                .WithMessage("Expiry date must be in the future");
            
            RuleFor(x => x.Permissions)
                .NotEmpty().WithMessage("At least one permission must be assigned to the API key");
        }
    }
}