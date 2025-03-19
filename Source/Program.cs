using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.ApiKey;
using MultiSoftSRB.Auth.Permissions;
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

builder.Services.AddDataSeeding();

var app = builder.Build();
app.UseCors("AllowReact");
app.UseAuthentication()
   .UseAuthorization()
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
           };
       });

app.UseOpenApi(c => c.Path = "/openapi/{documentName}.json");    
app.MapScalarApiReference();

app.ApplyMigrations();

await app.SeedDataAsync();

app.Run();

public partial class Program;