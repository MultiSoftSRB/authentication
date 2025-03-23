using FluentValidation;

namespace MultiSoftSRB.Features.Agency.Clients.AssignUser;

sealed class Request
{
    public long CompanyId { get; set; }
    public long UserId { get; set; }
    public long RoleId { get; set; }

    internal sealed class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");
            
            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("Company ID is required");

            RuleFor(x => x.RoleId)
                .NotEmpty().WithMessage("Role name is required");
        }
    }
}