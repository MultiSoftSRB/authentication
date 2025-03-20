using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Auth.ApiKeys.GetKeys;

sealed class Endpoint : EndpointWithoutRequest<List<Response>>
{
    public MainDbContext MainDbContext { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    
    public override void Configure()
    {
        Get("auth/api-keys");
        Permissions(ResourcePermissions.ApiKeyList);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var companyId = CompanyProvider.GetCompanyId();
        var apiKeys = await MainDbContext.ApiKeys
            .Where(k => k.CompanyId == companyId)
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