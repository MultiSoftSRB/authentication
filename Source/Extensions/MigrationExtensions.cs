using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Database.Audit;
using MultiSoftSRB.Database.Company;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        // Apply migrations for MainDbContext
        var mainDbContext = services.GetRequiredService<MainDbContext>();
        Console.WriteLine("Applying migrations for MainDbContext...");
        mainDbContext.Database.Migrate();

        // Apply migrations for all company databases (CompanyMigrationDbContext)
        var companyConnectionStrings = services.GetRequiredService<IOptions<CompanyConnectionStrings>>();
        foreach (var connectionString in companyConnectionStrings.Value.Values)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CompanyMigrationDbContext>();
            optionsBuilder.UseNpgsql(connectionString.Value);

            using var dbContext = new CompanyMigrationDbContext(optionsBuilder.Options, null);
            
            Console.WriteLine($"Applying migrations for {(DatabaseType)connectionString.Key} database...");
            dbContext.Database.Migrate();
        }
        
        // Apply migrations for AuditDbContext
        var auditDbContext = services.GetRequiredService<AuditDbContext>();
        Console.WriteLine("Applying migrations for AuditDbContext...");
        auditDbContext.Database.Migrate();

        Console.WriteLine("All migrations applied successfully.");
    }
}