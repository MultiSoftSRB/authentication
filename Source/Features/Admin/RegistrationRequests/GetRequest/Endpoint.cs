using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Admin.RegistrationRequests.GetRequest;

sealed class Endpoint : EndpointWithoutRequest<Response>
{
    public MainDbContext MainDbContext { get; set; }
    
    public override void Configure()
    {
        Get("admin/registration-requests/{id}");
        Policies(CustomPolicies.SuperAdminOnly);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var id =  Route<long>("id");
        var request = await MainDbContext.RegistrationRequests
            .Where(rr => rr.Id == id)
            .Select(
                rr => new Response
                {
                    Id = rr.Id,
                    Timestamp = rr.Timestamp,
                    Email = rr.Email,
                    FirstName = rr.FirstName,
                    LastName = rr.LastName,
                    UserNameWithoutCompanyCode = rr.UserNameWithoutCompanyCode,
                    CompanyName = rr.CompanyName,
                    CompanyCode = rr.CompanyCode,
                    CompanyType = rr.CompanyType
                })
            .SingleOrDefaultAsync(cancellationToken);

        if (request == null)
        {
            AddError("Registration request not found");
            await SendErrorsAsync(StatusCodes.Status404NotFound, cancellationToken);
            return;
        }
        
        await SendOkAsync(request, cancellationToken);
    }
}