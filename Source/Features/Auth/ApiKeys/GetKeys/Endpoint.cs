using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Auth.ApiKeys.GetKeys;

sealed class Request
{
    [QueryParam]
    public long? CompanyId { get; set; }
}

sealed class Endpoint : Endpoint<Request, List<Response>>
{
    public MainDbContext MainDbContext { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    
    public override void Configure()
    {
        Get("auth/api-keys");
        Permissions(ResourcePermissions.ApiKeyList);
        Policies(CustomPolicies.SuperAdminOnly);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var apiKeys = await MainDbContext.ApiKeys
            .Where(k => (request.CompanyId == null || k.CompanyId == request.CompanyId))
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new Response
            {
                Id = k.Id,
                Name = k.Name,
                CreatedAt = k.CreatedAt,
                ExpiresAt = k.ExpiresAt,
                CreatedBy = k.CreatedByUser.FirstName + " " + k.CreatedByUser.LastName,
                Permissions = k.Permissions.Select(p => p.ResourcePermissionCode).ToList()
            })
            .ToListAsync(cancellationToken);

        await SendOkAsync(apiKeys, cancellationToken);
    }
}