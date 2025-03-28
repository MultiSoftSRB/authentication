using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Audit;

public static class AuditExclusionConfiguration
{
    public static void ConfigureExclusions(AuditOptions options)
    {
        // Exclude whole entity from auditing
        options.ExcludeEntity<RefreshToken>();
        
        // Exclude specific property of entity from auditing
        options.ExcludeProperty<ApiKey>(u => u.KeyHash);
        
        options.ExcludeProperty<User>(u => u.ConcurrencyStamp);
        options.ExcludeProperty<User>(u => u.SecurityStamp);
        options.ExcludeProperty<User>(u => u.LockoutEnabled);
        options.ExcludeProperty<User>(u => u.LockoutEnd);
        options.ExcludeProperty<User>(u => u.PasswordHash);
        options.ExcludeProperty<User>(u => u.AccessFailedCount);
        options.ExcludeProperty<User>(u => u.PhoneNumberConfirmed);
    }
}