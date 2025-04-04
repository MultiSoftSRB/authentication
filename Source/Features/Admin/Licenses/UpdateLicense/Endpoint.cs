using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Features.Admin.Licenses.UpdateLicense;

sealed class Endpoint : Endpoint<Request>
{
    public MainDbContext MainDbContext { get; set; }
    
    public override void Configure()
    {
        Put("admin/licenses/{id}");
        Permissions(ResourcePermissions.LicenseUpdate);
        Policies(CustomPolicies.SuperAdminOnly);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        // Validate features
        var allValidFeaturePermissions = FeaturePermissions.GetAll().ToHashSet();
        var invalidFeature = request.Features
            .Where(p => !allValidFeaturePermissions.Contains(p))
            .ToList();

        if (invalidFeature.Count != 0)
            ThrowError($"Invalid features: {string.Join(", ", invalidFeature)}");
        
        var licenseId = Route<int>("id");
        var license = await MainDbContext.Licenses
            .Where(l => l.Id == licenseId)
            .Include(r => r.Features)
            .FirstOrDefaultAsync(cancellationToken);

        if (license == null)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }
        
        // Get existing feature codes
        var existingFeatureCodes = license.Features
            .Select(p => p.FeaturePermissionCode)
            .ToHashSet();

        // Determine features to add and remove
        var featuresToAdd = request.Features
            .Where(p => !existingFeatureCodes.Contains(p))
            .ToList();

        var featuresToRemove = license.Features
            .Where(p => !request.Features.Contains(p.FeaturePermissionCode))
            .ToList();

        // Remove features that are no longer needed
        if (featuresToRemove.Count != 0)
            MainDbContext.RemoveRange(featuresToRemove);
        
        // Add new features
        foreach (var feature in featuresToAdd)
        {
            license.Features.Add(new LicenseFeature
            {
                LicenseId = license.Id,
                FeaturePermissionCode = feature
            });
        }

        await MainDbContext.SaveChangesAsync(cancellationToken);

        await SendOkAsync(cancellationToken);
    }
}