using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Agency.Clients.GetRoles;

sealed class Endpoint : EndpointWithoutRequest<List<Response>>
{
    public MainDbContext MainDbContext { get; set; }
    public UserProvider UserProvider { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    
    public override void Configure()
    {
        Get("agency/companies/{id}/roles");
        Permissions(ResourcePermissions.RoleList);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var clientCompanyId = Route<long>("id");
        var agencyCompanyId = CompanyProvider.GetCompanyId();

        var isAgenciesClient = await MainDbContext.AgencyClients.AnyAsync(
            ac => ac.AgencyCompanyId == agencyCompanyId && ac.ClientCompanyId == clientCompanyId,
            cancellationToken);

        if (!isAgenciesClient)
        {
            AddError("Provided company is not an agency client");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }

        var roles = await MainDbContext.Roles
            .Where(r => r.CompanyId == clientCompanyId)
            .Select(r => new Response
            {
                Id = r.Id,
                Name = r.Name
            })
            .ToListAsync(cancellationToken);

        await SendOkAsync(roles, cancellationToken);
    }
}