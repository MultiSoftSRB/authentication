using MultiSoftSRB.Auth;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Auth.GetAllowedCompanies;

sealed class Endpoint : EndpointWithoutRequest<List<Response>>
{
    public MainDbContext MainDbContext { get; set; }
    public UserProvider UserProvider { get; set; }
    
    public override void Configure()
    {
        Get("auth/allowed-companies");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var userId = UserProvider.GetCurrentUserId();
        var companies = MainDbContext.UserCompanies
            .Where(u => u.UserId == userId)
            .Select(uc => new Response
            {
                Id = uc.CompanyId,
                Name = uc.Company.Name
            })
            .ToList();
        
        await SendOkAsync(companies, cancellation: cancellationToken);
    }
}