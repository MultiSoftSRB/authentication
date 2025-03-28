using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Entities.Audit;

namespace MultiSoftSRB.Database.Audit;

public abstract class BaseAuditDbContext : DbContext
{
    // Default audit logs table for entities without specific mapping
    public DbSet<DefaultAuditLog> DefaultAuditLogs { get; set; }
    
    protected BaseAuditDbContext(DbContextOptions options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditLog>().UseTpcMappingStrategy();
        
        // Apply configuration for all entities, that are defined in specific namespace/folder
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseAuditDbContext).Assembly,
            t => t.Namespace == "MultiSoftSRB.Database.Audit.Configurations");
    }
}