using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.ApiKey;
using MultiSoftSRB.Database.Audit;
using MultiSoftSRB.Entities.Audit;

namespace MultiSoftSRB.Extensions;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ApiKeyProvider _apiKeyProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuditDbContext _auditContext;
    
    // Dictionary to track new entities and their temporary IDs
    private readonly Dictionary<object, string> _temporaryEntityIds = new();
    
    public AuditSaveChangesInterceptor(
        IServiceProvider serviceProvider,
        ApiKeyProvider apiKeyProvider,
        IHttpContextAccessor httpContextAccessor,
        AuditDbContext auditContext)
    {
        _serviceProvider = serviceProvider;
        _apiKeyProvider = apiKeyProvider;
        _httpContextAccessor = httpContextAccessor;
        _auditContext = auditContext;
    }
    
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is null)
            return base.SavingChanges(eventData, result);
            
        BeforeSaveChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }
    
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (eventData.Context is null)
            return base.SavedChanges(eventData, result);
            
        AfterSaveChanges(eventData.Context);
        return base.SavedChanges(eventData, result);
    }
    
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
            
        BeforeSaveChanges(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, 
        int result, 
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return await base.SavedChangesAsync(eventData, result, cancellationToken);
            
        AfterSaveChanges(eventData.Context);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }
    
    private void BeforeSaveChanges(DbContext context)
    {
        // Skip audit context itself to avoid infinite loops
        if (context is AuditDbContext)
            return;
            
        // Track entities that are being added to capture their temporary IDs
        var addedEntries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added)
            .ToList();
            
        foreach (var entry in addedEntries)
        {
            // Store a reference to the entity and its temporary ID
            var temporaryId = GetEntityId(entry);
            _temporaryEntityIds[entry.Entity] = temporaryId;
        }
    }
    
    private void AfterSaveChanges(DbContext context)
    {
        // Skip audit context itself to avoid infinite loops
        if (context is AuditDbContext)
            return;
            
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();
            
        if (entries.Count == 0)
            return;
            
        var timestamp = DateTime.UtcNow;
        var contextType = context.GetType().Name;
        var endpoint = GetCurrentEndpoint();
        
        // Determine authentication type and identity
        var (userId, userName, apiKeyId, companyId, isApiKeyAuth) = GetAuthenticationInfo();
        
        var auditLogs = new List<AuditLog>();
        
        foreach (var entry in entries)
        {
            var entityId = GetEntityId(entry);
            
            // if (entry.State == EntityState.Added)
            // {
            //     // For added entities, get the real ID that was generated
            //     entityId = GetEntityId(entry);
            // }
            // else
            // {
            //     entityId = GetEntityId(entry);
            // }
            
            var oldValues = entry.State != EntityState.Added 
                ? SerializeEntityValues(entry, true) 
                : null;
                
            var newValues = entry.State != EntityState.Deleted 
                ? SerializeEntityValues(entry, false) 
                : null;
                
            // Create property-by-property change objects
            var changedProps = entry.State == EntityState.Modified 
                ? CreateChangedPropertiesObject(entry) 
                : null;
            
            var auditLog = new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                ActionType = ConvertToAuditActionType(entry.State),
                Timestamp = timestamp,
                
                // Authentication info
                UserId = isApiKeyAuth ? null : userId,
                UserName = isApiKeyAuth ? null : userName,
                IsApiKeyAuth = isApiKeyAuth,
                ApiKeyId = isApiKeyAuth ? apiKeyId : null,
                
                CompanyId = companyId,
                EntityId = entityId,
                Endpoint = endpoint,
                ContextType = contextType,
                OldValues = oldValues,
                NewValues = newValues,
                ChangedProperties = changedProps
            };
            
            auditLogs.Add(auditLog);
        }
        
        _auditContext.AuditLogs.AddRange(auditLogs);
        _auditContext.SaveChanges();
        
        // Clear the temporary ID tracking dictionary
        _temporaryEntityIds.Clear();
    }
    
    private (long? UserId, string? UserName, long? ApiKeyId, long? CompanyId, bool IsApiKeyAuth) GetAuthenticationInfo()
    {
        // Check if we're using API key authentication
        var apiKeyInfo = _apiKeyProvider.GetCurrentApiKey();
        if (apiKeyInfo != null)
        {
            return (
                UserId: null,
                UserName: null,
                ApiKeyId: apiKeyInfo.Id,
                CompanyId: apiKeyInfo.CompanyId,
                IsApiKeyAuth: true
            );
        }
        
        // Otherwise, get user information from UserProvider
        var userProvider = _serviceProvider.GetRequiredService<UserProvider>();
        var userId = userProvider.GetCurrentUserId();
        var userName = userProvider.GetCurrentUserName();
        var companyId = userProvider.GetCurrentCompanyId();
        
        return (
            UserId: userId,
            UserName: userName,
            ApiKeyId: null,
            CompanyId: companyId,
            IsApiKeyAuth: false
        );
    }
    
    private string GetCurrentEndpoint()
    {
        if (_httpContextAccessor.HttpContext == null)
            return "Unknown";
            
        var endpoint = _httpContextAccessor.HttpContext.GetEndpoint();
        if (endpoint == null)
            return "Unknown";
            
        // If using FastEndpoints, try to extract the endpoint name
        var fastEndpoint = endpoint.Metadata.GetMetadata<EndpointDefinition>();
        if (fastEndpoint != null)
            return fastEndpoint.GetType().FullName;
        
        return $"{_httpContextAccessor.HttpContext?.Request.Method} {_httpContextAccessor.HttpContext?.Request?.Path.Value}";
    }
    
    private string GetClientIp()
    {
        if (_httpContextAccessor.HttpContext == null)
            return null;
            
        return _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
    }
    
    private string GetEntityId(EntityEntry entry)
    {
        // Get primary key properties
        var keyProperties = entry.Metadata.FindPrimaryKey()?.Properties;
        if (keyProperties == null || !keyProperties.Any())
            return "Unknown";
        
        var keyValues = new List<string>();
        
        foreach (var property in keyProperties)
        {
            var keyValue = entry.Property(property.Name).CurrentValue?.ToString();
            if (keyValue != null)
                keyValues.Add(keyValue);
        }
        
        return keyValues.Count > 0 ? string.Join("-", keyValues) : "Unknown";
    }
    
    private string SerializeEntityValues(EntityEntry entry, bool useOriginalValues = false)
    {
        var values = new Dictionary<string, object>();
        
        foreach (var property in entry.Properties)
        {
            // Skip shadow properties
            if (property.Metadata.IsShadowProperty())
                continue;
                
            // Skip navigation properties
            if (property.Metadata.IsShadowProperty() || property.Metadata.IsIndexerProperty())
                continue;
                
            var value = useOriginalValues ? property.OriginalValue : property.CurrentValue;
            values[property.Metadata.Name] = value;
        }
        
        return JsonSerializer.Serialize(values, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });
    }
    
    private string CreateChangedPropertiesObject(EntityEntry entry)
    {
        var changedProperties = new Dictionary<string, object>();
        
        foreach (var property in entry.Properties)
        {
            // Skip unchanged properties
            if (!property.IsModified)
                continue;
                
            // Skip shadow properties
            if (property.Metadata.IsShadowProperty())
                continue;
                
            // Skip navigation properties
            if (property.Metadata.IsShadowProperty() || property.Metadata.IsIndexerProperty())
                continue;
                
            changedProperties[property.Metadata.Name] = new 
            {
                OldValue = property.OriginalValue,
                NewValue = property.CurrentValue
            };
        }
        
        return JsonSerializer.Serialize(changedProperties, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });
    }
    
    private ActionType ConvertToAuditActionType(EntityState state)
    {
        return state switch
        {
            EntityState.Added => ActionType.Create,
            EntityState.Modified => ActionType.Update,
            EntityState.Deleted => ActionType.Delete,
            _ => ActionType.Read
        };
    }
}