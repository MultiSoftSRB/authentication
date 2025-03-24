using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Entities.Company;

namespace MultiSoftSRB.Database.Company;

public class CompanyMigrationDbContext : DbContext
{
    private readonly CompanyConnectionStrings? _connectionStrings;
    
    public CompanyMigrationDbContext(DbContextOptions<CompanyMigrationDbContext> options, IOptions<CompanyConnectionStrings>? connectionStrings) : base(options)
    {
        _connectionStrings = connectionStrings?.Value;
    }

    public DbSet<Article> Articles { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_connectionStrings != null)
            optionsBuilder.UseNpgsql(_connectionStrings.Values.First().Value);
    }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Apply configuration for all entities, that are defined in specific namespace/folder
        builder.ApplyConfigurationsFromAssembly(typeof(CompanyDbContext).Assembly,
            t => t.Namespace == "MultiSoftSRB.Database.Company.Configurations");
        
        // Apply global query filter for all entities inheriting CompanyEntity
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(CompanyEntity).IsAssignableFrom(entityType.ClrType))
            {
                builder.Entity(entityType.ClrType).HasIndex(nameof(CompanyEntity.CompanyId));
            }
        }
    }
}