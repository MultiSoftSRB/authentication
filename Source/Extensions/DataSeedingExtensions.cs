using MultiSoftSRB.Services;

namespace MultiSoftSRB.Extensions;

public static class DataSeedingExtensions
{
    /// <summary>
    /// Extension method to add data seeding services to the service collection
    /// </summary>
    public static IServiceCollection AddDataSeeding(this IServiceCollection services)
    {
        services.AddScoped<DataSeeder>();
        return services;
    }
        
    /// <summary>
    /// Extension method to run data seeding during application startup
    /// </summary>
    public static async Task SeedDataAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
            
        try
        {
            var seeder = services.GetRequiredService<DataSeeder>();
            await seeder.SeedAllAsync();
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<IHost>>();
            logger.LogError(ex, "An error occurred while seeding the database");
        }
    }
}