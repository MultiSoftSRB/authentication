using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Auth.Users.GetUsers;

sealed class Endpoint : EndpointWithoutRequest<List<Response>>
{
    public MainDbContext MainDbContext { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    
    public override void Configure()
    {
        Get("auth/users");
        Permissions(ResourcePermissions.UserList);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var companyId = CompanyProvider.GetCompanyId();
        var users = await MainDbContext.Users
            .Where(u => u.UserCompanies.Any(uc => uc.CompanyId == companyId))
            .Select(u => new Response
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.UserRoles
                    .Where(ur => ur.CompanyId == companyId)
                    .Select(ur => ur.Role.Name)
                    .FirstOrDefault() ?? "No Role"
            })
            .ToListAsync(cancellationToken);
        
        await SendOkAsync(users, cancellationToken);
    }
}