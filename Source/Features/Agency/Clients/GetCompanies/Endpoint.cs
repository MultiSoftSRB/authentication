using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Agency.Clients.GetCompanies;

sealed class Endpoint : EndpointWithoutRequest<List<Response>>
{
    public MainDbContext MainDbContext { get; set; }
    public UserProvider UserProvider { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    
    public override void Configure()
    {
        Get("agency/companies");
        Permissions(ResourcePermissions.CompanyList);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var currentUsersCompanyId = CompanyProvider.GetCompanyId();
        var companies = await MainDbContext.AgencyClients
            .Where(ac => ac.AgencyCompanyId == currentUsersCompanyId)
            .Select(
                ac => new Response
                {
                    Id = ac.ClientCompany.Id,
                    Name = ac.ClientCompany.Name,
                    Code = ac.ClientCompany.Code
                })
            .ToListAsync(cancellationToken);

        await SendOkAsync(companies, cancellationToken);
    }
}