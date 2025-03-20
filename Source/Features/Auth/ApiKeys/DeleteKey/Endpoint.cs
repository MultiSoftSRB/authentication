using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Auth.ApiKeys.DeleteKey;

sealed class Endpoint : EndpointWithoutRequest
{
    public MainDbContext MainDbContext { get; set; }
    
    public override void Configure()
    {
        Delete("auth/api-keys/{id}");
        Permissions(ResourcePermissions.ApiKeyDelete);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var keyId = Route<long>("id");
        
        var apiKey = await MainDbContext.ApiKeys
            .Where(k => k.Id == keyId)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (apiKey == null)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }
        
        // Revoke the API key
        MainDbContext.Remove(apiKey);
        await MainDbContext.SaveChangesAsync(cancellationToken);
        
        await SendOkAsync(cancellationToken);
    }
}