using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using MultiSoftSRB.Auth.ApiKey;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Extensions;

public static class AuthSetupExtensions
{
    public static void SetupAuthentication(this IServiceCollection services, ConfigurationManager configurationManager)
    {
        services.AddScoped<SignInManager<User>>();
        services
            .AddAuthentication(options => 
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(
                o =>
                {
                    var jwtSettings = configurationManager.GetSection("JwtSettings");
                    SecurityKey key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings["Secret"]));

                    //set defaults
                    o.TokenValidationParameters.IssuerSigningKey = key;
                    o.TokenValidationParameters.ValidIssuer = jwtSettings["Issuer"];
                    o.TokenValidationParameters.ValidateIssuer = true;
                    o.TokenValidationParameters.ValidateIssuerSigningKey = true;
                    o.TokenValidationParameters.ValidateLifetime = true;
                    o.TokenValidationParameters.ClockSkew = TimeSpan.FromSeconds(60);
                    o.TokenValidationParameters.ValidAudience = jwtSettings["Audience"];
                    o.TokenValidationParameters.ValidateAudience = true;

                    o.MapInboundClaims = false;
                })
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationHandler.SchemeName, null);
        
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, ApiKeyAuthenticationHandler.SchemeName)
                .RequireAuthenticatedUser()
                .Build();
        });
    }
}