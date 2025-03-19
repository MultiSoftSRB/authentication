using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Features.Auth.ApiKeys.CreateKey;

sealed class Endpoint : Endpoint<Request, Response>
{
    public MainDbContext MainDbContext { get; set; }
    public UserProvider UserProvider { get; set; }
    
    public override void Configure()
    {
        Post("auth/api-keys");
        Permissions(ResourcePermissions.ApiKeyCreate);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var userId = UserProvider.GetCurrentUserId();
        
        // Validate permissions
        var allValidResourcePermissions = ResourcePermissions.GetAll().ToHashSet();
        var invalidPermissions = request.Permissions
            .Where(p => !allValidResourcePermissions.Contains(p))
            .ToList();

        if (invalidPermissions.Count != 0)
            ThrowError($"Invalid permissions: {string.Join(", ", invalidPermissions)}");
        
        if (!await MainDbContext.Companies.AnyAsync(c => c.Id == request.CompanyId, cancellationToken))
            ThrowError("Company with provided ID does not exist", StatusCodes.Status404NotFound);
        
        // Generate a unique API key
        var apiKeyValue = GenerateApiKey();
        var apiKeyHash = HashApiKey(apiKeyValue);
        
        // Create the API key record
        var apiKey = new ApiKey
        {
            CompanyId = request.CompanyId,
            Name = request.Name,
            KeyHash = apiKeyHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            CreatedBy = userId
        };
        
        // Add permissions
        foreach (var permission in request.Permissions)
        {
            apiKey.Permissions.Add(new ApiKeyPermission
            {
                ResourcePermissionCode = permission
            });
        }
        
        await MainDbContext.ApiKeys.AddAsync(apiKey, cancellationToken);
        await MainDbContext.SaveChangesAsync(cancellationToken);
            
        var response = new Response
        {
            Id = apiKey.Id,
            Key = apiKeyValue
        };
        
        await SendOkAsync(response, cancellation: cancellationToken);
    }
    
    // Helper method to generate a cryptographically secure API key
    private string GenerateApiKey()
    {
        var bytes = new byte[32]; // 256 bits
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        
        // Prefix with "xy_" to make it easily identifiable
        return "xy_" + Convert.ToBase64String(bytes)
            .Replace("/", "_")
            .Replace("+", "-")
            .Replace("=", "");
    }
    
    // Helper method to hash an API key for secure storage
    private string HashApiKey(string apiKey)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(bytes);
    }
}