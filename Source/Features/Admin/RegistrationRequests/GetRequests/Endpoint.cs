using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Admin.RegistrationRequests.GetRequests;

sealed class Endpoint : EndpointWithoutRequest<List<Response>>
{
    public MainDbContext MainDbContext { get; set; }
    
    public override void Configure()
    {
        Get("admin/registration-requests");
        Policies(CustomPolicies.SuperAdminOnly);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var requests = await MainDbContext.RegistrationRequests
            .Select(
                rr => new Response
                {
                    Id = rr.Id,
                    CompanyName = rr.CompanyName,
                    UserFullName = rr.FirstName + " " + rr.LastName,
                    Email = rr.Email,
                    Timestamp = rr.Timestamp
                })
            .ToListAsync(cancellationToken);
        
        await SendOkAsync(requests, cancellationToken);
    }
}