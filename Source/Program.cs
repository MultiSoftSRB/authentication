using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Audit;
using Scalar.AspNetCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.ApiKey;
using MultiSoftSRB.Auth.Licensing;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Audit;
using MultiSoftSRB.Database.Company;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;
using MultiSoftSRB.Extensions;
using MultiSoftSRB.Services;
using NSwag;

var builder = WebApplication.CreateBuilder(args);

// Register HTTP context accessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddFusionCache();

builder.Services
       .AddTransient<IClaimsTransformation, UserPermissionClaimHydrator>()
       .AddTransient<TokenService>()
       .AddTransient<UserProvider>()
       .AddTransient<CompanyProvider>()
       .AddTransient<ApiKeyProvider>()
       .AddTransient<LicenseProvider>()
       .AddTransient<RolesService>();

#region DbContext Setup

builder.Services.AddScoped<AuditSaveChangesInterceptor>();

builder.Services.AddDbContext<MainDbContext>((serviceProvider, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("MainDatabase"));
    options.AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
});

// Register the audit db contexts and options for excluding entities and properties from audit
builder.Services.AddDbContext<MainAuditDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("MainAuditDatabase"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});
builder.Services.AddDbContext<CompanyAuditDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("CompanyAuditDatabase"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});
builder.Services.AddSingleton<AuditOptions>();
        
builder.Services.AddSingleton<AuditOptions>(_ => {
    var options = new AuditOptions();
    AuditExclusionConfiguration.ConfigureExclusions(options);
    return options;
});

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policyConfig =>
    {
        policyConfig.WithOrigins(builder.Configuration["FrontendUrl"])
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

builder.Services.SetupAuthentication(builder.Configuration);
builder.Services
    .AddFastEndpoints(o => o.SourceGeneratorDiscoveredTypes = DiscoveredTypes.All)
    .SwaggerDocument(o =>
    {
        o.EnableJWTBearerAuth = false;
        o.DocumentSettings = s =>
        {
            s.AddAuth("ApiKey", new()
            {
                Name = ApiKeyAuthenticationHandler.HeaderName,
                In = OpenApiSecurityApiKeyLocation.Header,
                Type = OpenApiSecuritySchemeType.ApiKey,
            });
            s.AddAuth("Bearer", new()
            {
                Type = OpenApiSecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
            });
        };
    });

// Enable App Insights only for testing and production
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor(options => 
    {
        options.ConnectionString = builder.Configuration.GetConnectionString("AppInsights");
    });
}

builder.Services.AddDataSeeding();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseCors("AllowReact");
app.UseAuthentication()
   .UseAuthorization()
   .UseMiddleware<SessionValidationMiddleware>()
   .UseDefaultExceptionHandler(null, app.Environment.IsProduction(), app.Environment.IsProduction())
   .UseFastEndpoints(
       c =>
       {
           c.Binding.ReflectionCache.AddFromMultiSoftSRB();
           c.Errors.UseProblemDetails();
           c.Security.PermissionsClaimType = CustomClaimTypes.ResourcePermission;
           c.Endpoints.Configurator = ep =>
           {
               // Set default auth schemas for all endpoints
               ep.AuthSchemes(JwtBearerDefaults.AuthenticationScheme, ApiKeyAuthenticationHandler.SchemeName);
               
               // Set description for all endpoints that show which class is used as response in case of 500 status code
               ep.Description(b => b.Produces<InternalErrorResponse>(500));
           };
       });

app.UseOpenApi(c => c.Path = "/openapi/{documentName}.json");    
app.MapScalarApiReference("/");

app.ApplyMigrations();

await app.SeedDataAsync();

app.Run();

public partial class Program;