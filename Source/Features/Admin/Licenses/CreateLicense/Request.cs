using FluentValidation;

namespace MultiSoftSRB.Features.Admin.Licenses.CreateLicense;

sealed class Request
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public List<string> Features { get; set; } = [];
    
    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(64)
                .WithMessage("License name is required and must be less than 64 characters");

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(256)
                .WithMessage("Description is required and must be less than 256 characters");

            RuleFor(x => x.Price)
                .NotEmpty()
                .GreaterThanOrEqualTo(0)
                .WithMessage("Price is required and cannot be negative");
        }
    }
}