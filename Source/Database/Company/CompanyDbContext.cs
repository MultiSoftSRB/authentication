using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Entities.Company;

namespace MultiSoftSRB.Database.Company;

public class CompanyDbContext : DbContext
{
    private readonly CompanyProvider _companyProvider;
    private readonly long _companyId;

    public CompanyDbContext(DbContextOptions<CompanyDbContext> options, CompanyProvider companyProvider) : base(options)
    {
        _companyProvider = companyProvider;
        _companyId = companyProvider.GetCompanyId();
    }

    public DbSet<Article> Articles { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_companyProvider.GetConnectionString());
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
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var companyIdProperty = Expression.Property(parameter, nameof(CompanyEntity.CompanyId));
                var companyIdValue = Expression.Constant(_companyId);
                var predicate = Expression.Lambda(Expression.Equal(companyIdProperty, companyIdValue), parameter);

                builder.Entity(entityType.ClrType).HasQueryFilter(predicate);
                builder.Entity(entityType.ClrType).HasIndex(nameof(CompanyEntity.CompanyId));
            }
        }
        
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