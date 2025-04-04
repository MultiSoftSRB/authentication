using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Database.Main;

public class MainDbContext : DbContext
{
    public MainDbContext(DbContextOptions<MainDbContext> options) : base(options) { }

    public DbSet<Entities.Main.Company> Companies { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserCompany> UserCompanies { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<ApiKey> ApiKeys { get; set; }
    public DbSet<ApiKeyPermission> ApiKeyPermissions { get; set; }
    public DbSet<AgencyClient> AgencyClients { get; set; }
    public DbSet<License> Licenses { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Apply configuration for all entities, that are defined in specific namespace/folder
        builder.ApplyConfigurationsFromAssembly(typeof(MainDbContext).Assembly,
            t => t.Namespace == "MultiSoftSRB.Database.Main.Configurations");
        
        // DateTime configuration for PostgreSQL, as it only allows storing timestamp dates in UTC
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                        v => v.ToUniversalTime(), // Convert to UTC before saving
                        v => DateTime.SpecifyKind(v, DateTimeKind.Local) // Convert to Local timezone when reading
                    ));
                }
            }
        }
    }
}