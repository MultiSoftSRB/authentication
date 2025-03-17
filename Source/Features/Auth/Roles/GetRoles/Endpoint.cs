using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Auth.Roles.GetRoles;

sealed class Endpoint : EndpointWithoutRequest<List<Response>>
{
    public MainDbContext MainDbContext { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    
    public override void Configure()
    {
        Get("auth/roles");
        Permissions(ResourcePermissions.RoleList);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var companyId = CompanyProvider.GetCompanyId();
        var roles = await MainDbContext.Roles
            .Where(r => r.CompanyId == companyId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var response = roles.Select(
            r => new Response
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            }).ToList();

        await SendOkAsync(response, cancellationToken);
    }
}