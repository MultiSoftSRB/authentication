using FluentValidation;

namespace MultiSoftSRB.Features.Agency.Clients.RevokeUser;

sealed class Request
{
    public long UserId { get; set; }
    public long CompanyId { get; set; }
    
    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");
            
            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("Company ID is required");
        }
    }
}