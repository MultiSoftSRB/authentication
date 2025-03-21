using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Features.Admin.Consultants.GetConsultants;

sealed class Endpoint : EndpointWithoutRequest<List<Response>>
{
    public MainDbContext MainDbContext { get; set; }
    
    public override void Configure()
    {
        Get("admin/consultants");
        Permissions(ResourcePermissions.UserList);
        Policies(CustomPolicies.SuperAdminOnly);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var response = await MainDbContext.Users
            .Where(u => u.UserType == UserType.Consultant)
            .Select(u => new Response
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                FullName = $"{u.FirstName} {u.LastName}",
                PhoneNumber = u.PhoneNumber
            })
            .ToListAsync(cancellationToken);
            
        await SendOkAsync(response, cancellationToken);
    }
}