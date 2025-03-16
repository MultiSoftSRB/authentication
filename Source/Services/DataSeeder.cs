using Microsoft.AspNetCore.Identity;
using MultiSoftSRB.Entities.Main;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Services;

public class DataSeeder
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    
    public DataSeeder(UserManager<User> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }
    
    public async Task SeedAllAsync()
    {
        await SeedSuperAdminAsync();
    }
    
    private async Task SeedSuperAdminAsync()
    {
        // Check if admin user already exists
        var adminSection = _configuration.GetSection("SuperAdmin");
        string? username = adminSection["Username"];
        string? email = adminSection["Email"];
        
        if (string.IsNullOrEmpty(username))
        {
            Console.WriteLine("No SuperAdmin username configured in settings. Skipping admin seeding.");
            return;
        }
        
        if (string.IsNullOrEmpty(email))
        {
            Console.WriteLine("No SuperAdmin email configured in settings. Skipping admin seeding.");
            return;
        }
        
        var existingAdmin = await _userManager.FindByNameAsync(username);
        if (existingAdmin != null)
        {
            Console.WriteLine("Super admin user already exists - skipping creation");
            return;
        }
        
        Console.WriteLine("Creating the super admin user");
        
        var adminUser = new User
        {
            UserName = username,
            NormalizedUserName = username.ToUpper(),
            Email = email,
            NormalizedEmail = email.ToUpper(),
            FirstName = adminSection["FirstName"] ?? "System",
            LastName = adminSection["LastName"] ?? "Administrator",
            UserType = UserType.SuperAdmin,
            EmailConfirmed = true,
        };
        
        string? adminPassword = adminSection["Password"];
        if (string.IsNullOrEmpty(adminPassword))
        {
            Console.WriteLine("No SuperAdmin password configured in settings. Cannot create admin user.");
            return;
        }
        
        var result = await _userManager.CreateAsync(adminUser, adminPassword);
        if (!result.Succeeded)
        {
            string errors = string.Join(", ", result.Errors.Select(e => e.Description));
            Console.WriteLine($"Failed to create super admin user: {errors}");
            return;
        }
        
        Console.WriteLine("Super admin user created successfully");
    }
}