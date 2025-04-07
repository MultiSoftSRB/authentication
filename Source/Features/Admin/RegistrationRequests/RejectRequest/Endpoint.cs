using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Admin.RegistrationRequests.RejectRequest;

sealed class Endpoint : EndpointWithoutRequest
{
    public MainDbContext MainDbContext { get; set; }
    
    public override void Configure()
    {
        Delete("admin/registration-requests/{id}");
        Policies(CustomPolicies.SuperAdminOnly);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var id =  Route<long>("id");
        var request = await MainDbContext.RegistrationRequests
            .Where(rr => rr.Id == id)
            .SingleOrDefaultAsync(cancellationToken);

        if (request == null)
        {
            AddError("Registration request not found");
            await SendErrorsAsync(StatusCodes.Status404NotFound, cancellationToken);
            return;
        }

        MainDbContext.RegistrationRequests.Remove(request);
        await MainDbContext.SaveChangesAsync(cancellationToken);
        
        await SendOkAsync(cancellationToken);
    }
}