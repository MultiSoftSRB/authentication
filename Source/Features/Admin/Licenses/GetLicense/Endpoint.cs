using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Admin.Licenses.GetLicense;

sealed class Endpoint : EndpointWithoutRequest<Response>
{
    public MainDbContext MainDbContext { get; set; }
    public override void Configure()
    {
        Get("admin/licenses/{id}");
        Permissions(ResourcePermissions.LicenseRead);
        Policies(CustomPolicies.SuperAdminOnly);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var licenseId = Route<long>("id");
        var license = await MainDbContext.Licenses
            .Include(l => l.Features)
            .Where(l => l.Id == licenseId)
            .Select(l => new Response
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                Price = l.Price,
                Features = l.Features.Select(f => f.FeaturePermissionCode).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
                
        if (license == null)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }
        
        await SendOkAsync(license, cancellationToken);
    }
}