using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Features.Admin.Licenses.CreateLicense;

sealed class Endpoint : Endpoint<Request, Response>
{
    public MainDbContext MainDbContext { get; set; }
    public override void Configure()
    {
        Post("admin/licenses");
        Permissions(ResourcePermissions.LicenseCreate);
        Policies(CustomPolicies.SuperAdminOnly);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        // Validate features
        var allValidFeaturePermissions = FeaturePermissions.GetAll().ToHashSet();
        var invalidFeatures = request.Features
            .Where(p => !allValidFeaturePermissions.Contains(p))
            .ToList();

        if (invalidFeatures.Count != 0)
            ThrowError($"Invalid features: {string.Join(", ", invalidFeatures)}");

        // Create new license
        var license = new License
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price
        };

        // Add features
        foreach (var feature in request.Features)
        {
            license.Features.Add(new LicenseFeature
            {
                FeaturePermissionCode = feature
            });
        }

        await MainDbContext.Licenses.AddAsync(license, cancellationToken);
        await MainDbContext.SaveChangesAsync(cancellationToken);

        var response = new Response
        {
            Id = license.Id,
            Name = license.Name
        };

        await SendOkAsync(response, cancellationToken);
    }
}