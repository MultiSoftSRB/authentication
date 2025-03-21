using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Admin.Companies.GetCompanies;

sealed class Endpoint : EndpointWithoutRequest<List<Response>>
{
    public MainDbContext MainDbContext { get; set; }
    
    public override void Configure()
    {
        Get("admin/companies");
        Policies(CustomPolicies.SuperAdminOnly);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var response = await MainDbContext.Companies.Select(c => new Response
        {
            Id = c.Id,
            Name = c.Name,
            Code = c.Code,
            CreatedAt = c.CreatedAt,
            DatabaseType = c.DatabaseType
        }).ToListAsync(cancellationToken);

        SendOkAsync(response, cancellationToken);
    }
}