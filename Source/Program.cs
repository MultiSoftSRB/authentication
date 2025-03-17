using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Company;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;
using MultiSoftSRB.Extensions;
using MultiSoftSRB.Services;

var builder = WebApplication.CreateBuilder(args);

// Register HTTP context accessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddFusionCache();

builder.Services
       .AddTransient<IClaimsTransformation, UserPermissionClaimHydrator>()
       .AddTransient<TokenService>()
       .AddTransient<UserProvider>()
       .AddTransient<CompanyProvider>();

#region DbContext Setup

builder.Services.AddDbContext<MainDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("MainDatabase")));

builder.Services
       .AddIdentityCore<User>(options =>
       {
           options.SignIn.RequireConfirmedEmail = false; 
           options.Password.RequiredLength = 8; 
           options.Password.RequireNonAlphanumeric = true;
       })
       .AddEntityFrameworkStores<MainDbContext>()
       .AddDefaultTokenProviders();

builder.Services.AddDbContext<CompanyDbContext>();
builder.Services.AddDbContext<CompanyMigrationDbContext>();

builder.Services.Configure<CompanyConnectionStrings>(options =>
    builder.Configuration.GetSection($"{nameof(CompanyConnectionStrings)}").Bind(options));

#endregion

builder.Services.AddScoped<SignInManager<User>>();
builder.Services
       .AddAuthenticationJwtBearer(s => s.SigningKey = builder.Configuration["JwtSettings:Secret"])
       .AddAuthorization()
       .AddFastEndpoints(o => o.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All)
       .SwaggerDocument();

builder.Services.Configure<JwtCreationOptions>( o =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    o.SigningKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT settings must be present.");
    o.Issuer = jwtSettings["Issuer"];
    o.Audience = jwtSettings["Audience"];
});

builder.Services.AddDataSeeding();

var app = builder.Build();
app.UseAuthentication()
   .UseAuthorization()
   .UseFastEndpoints(
       c =>
       {
           c.Binding.ReflectionCache.AddFromMultiSoftSRB();
           c.Errors.UseProblemDetails();
           c.Security.PermissionsClaimType = CustomClaimTypes.ResourcePermission;
       });

app.UseOpenApi(c => c.Path = "/openapi/{documentName}.json");    
app.MapScalarApiReference();

app.ApplyMigrations();

await app.SeedDataAsync();

app.Run();

public partial class Program;