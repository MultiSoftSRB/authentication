using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.ApiKey;
using MultiSoftSRB.Database.Audit;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Audit;

namespace MultiSoftSRB.Audit;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AuditOptions _auditOptions;
    private readonly Dictionary<EntityEntry, AuditLog> _pendingAudits = new();
    
    public AuditSaveChangesInterceptor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _auditOptions = _serviceProvider.GetRequiredService<AuditOptions>();
    }
    
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ProcessChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ProcessChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        UpdateInsertedEntitiesIds(eventData.Context);
        SaveAuditLogs(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        UpdateInsertedEntitiesIds(eventData.Context);
        SaveAuditLogs(eventData.Context);
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void ProcessChanges(DbContext? context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            // Skip if entity is excluded from auditing
            var entityType = entry.Entity.GetType();
            if (_auditOptions.IsEntityExcluded(entityType))
                continue;

            _pendingAudits[entry] = CreateAuditLog(entry);
        }
    }

    private void UpdateInsertedEntitiesIds(DbContext? context)
    {
        if (context == null) return;

        foreach (var (entry, auditLog) in _pendingAudits)
        {
            // Update the EntityId with the actual ID after it's been generated
            auditLog.EntityId = GetEntityId(entry);
        }
    }

    private void SaveAuditLogs(DbContext? context)
    {
        if (_pendingAudits.Count == 0 || context == null) 
            return;

        using var scope = _serviceProvider.CreateScope();
        
        // Group audit logs by entity type
        var auditsByEntityType = _pendingAudits
            .GroupBy(kv => kv.Key.Entity.GetType());
            
        BaseAuditDbContext auditContext = context is MainDbContext
            ? scope.ServiceProvider.GetRequiredService<MainAuditDbContext>()
            : scope.ServiceProvider.GetRequiredService<CompanyAuditDbContext>();
        
        foreach (var group in auditsByEntityType)
        {
            auditContext.AddRange(group.Select(g => g.Value));
        }
        
        auditContext.SaveChanges();
        
        _pendingAudits.Clear();
    }

    private AuditLog CreateAuditLog(EntityEntry entry)
    {
        var userProvider = _serviceProvider.GetService<UserProvider>();
        var apiKeyProvider = _serviceProvider.GetService<ApiKeyProvider>();
        var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();

        var actionType = entry.State switch
        {
            EntityState.Added => AuditActionType.Create,
            EntityState.Modified => AuditActionType.Update,
            EntityState.Deleted => AuditActionType.Delete,
            _ => AuditActionType.Update
        };

        var changedProperties = GetChangedProperties(entry);
        var httpRequest = httpContextAccessor?.HttpContext?.Request;
        var endpoint = $"{httpRequest?.Method} {httpRequest?.Path.Value}";
        var apiKey = apiKeyProvider?.GetCurrentApiKey();
        
        var entityType = entry.Entity.GetType();
        var auditLog = AuditTypeMapper.CreateAuditLogFor(entityType);
        auditLog.ActionType = actionType;
        auditLog.Timestamp = DateTime.UtcNow;
        auditLog.UserId = apiKey == null ? userProvider?.GetCurrentUserIdNullable() : null;
        auditLog.UserName = apiKey == null ? userProvider?.GetCurrentUserName() : null;
        auditLog.ApiKeyId = apiKey?.Id;
        auditLog.CompanyId = apiKey?.CompanyId ?? userProvider?.GetCurrentCompanyId();
        auditLog.EntityId = actionType == AuditActionType.Create ? "pending" : GetEntityId(entry);
        auditLog.Endpoint = endpoint;
        auditLog.ChangedProperties = JsonSerializer.Serialize(changedProperties);

        return auditLog;
    }

    private string GetEntityId(EntityEntry entry)
    {
        var keyValues = entry.Metadata.FindPrimaryKey()?.Properties
            .Select(p => entry.Property(p.Name).CurrentValue)
            .ToList();

        if (keyValues == null || !keyValues.Any())
            return "unknown";

        return keyValues.Count == 1 
            ? keyValues[0]?.ToString() ?? "null" 
            : JsonSerializer.Serialize(keyValues);
    }

    private List<PropertyChange> GetChangedProperties(EntityEntry entry)
    {
        var changes = new List<PropertyChange>();
        var entityType = entry.Entity.GetType();

        // If it's deleted, we don't need to track any fields, entity no longer exists
        if (entry.State == EntityState.Deleted)
            return changes;
        
        foreach (var property in entry.Properties)
        {
            // Skip if property is excluded from auditing
            if (_auditOptions.IsPropertyExcluded(entityType, property.Metadata.Name))
                continue;

            // For created entities, log all non-null properties
            if (entry.State == EntityState.Added && property.CurrentValue != null)
            {
                changes.Add(new PropertyChange
                {
                    PropertyName = property.Metadata.Name,
                    OldValue = null,
                    NewValue = property.CurrentValue
                });
            }
            // For modified entities, only log changed properties
            else if (entry.State == EntityState.Modified && property is { IsModified: true, OriginalValue: not null } && !property.OriginalValue.Equals(property.CurrentValue))
            {
                changes.Add(new PropertyChange
                {
                    PropertyName = property.Metadata.Name,
                    OldValue = property.OriginalValue,
                    NewValue = property.CurrentValue
                });
            }
        }

        return changes;
    }
}