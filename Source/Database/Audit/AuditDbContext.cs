using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Entities.Audit;

namespace MultiSoftSRB.Database.Audit;

public class AuditDbContext : DbContext
{
    public DbSet<AuditLog> AuditLogs { get; set; }

    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply configuration for all entities, that are defined in specific namespace/folder
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditDbContext).Assembly,
            t => t.Namespace == "MultiSoftSRB.Database.Audit.Configurations");
    }
}