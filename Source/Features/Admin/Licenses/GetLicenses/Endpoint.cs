using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Admin.Licenses.GetLicenses;

sealed class Endpoint : EndpointWithoutRequest<List<Response>>
{
    public MainDbContext MainDbContext { get; set; }
    
    public override void Configure()
    {
        Get("admin/licenses");
        Permissions(ResourcePermissions.LicenseList);
        Policies(CustomPolicies.SuperAdminOnly);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var licenses = await MainDbContext.Licenses.Select(l => new Response
        {
            Id = l.Id,
            Name = l.Name
        }).ToListAsync(cancellationToken);
        
        await SendOkAsync(licenses, cancellationToken);
    }
}