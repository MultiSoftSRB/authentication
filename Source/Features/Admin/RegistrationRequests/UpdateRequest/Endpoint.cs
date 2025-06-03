using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Admin.RegistrationRequests.UpdateRequest;


sealed class Endpoint : Endpoint<Request>
{
    public MainDbContext MainDbContext { get; set; }
    
    public override void Configure()
    {
        Put("admin/registration-requests/{id}");
        Policies(CustomPolicies.SuperAdminOnly);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var id =  Route<long>("id");

        var registrationRequest = await MainDbContext.RegistrationRequests
            .Where(rr => rr.Id == id)
            .SingleOrDefaultAsync(cancellationToken);
        
        if (registrationRequest == null)
        {
            AddError("Registration request not found");
            await SendErrorsAsync(StatusCodes.Status404NotFound, cancellationToken);
            return;
        }
        
        registrationRequest.FirstName = request.FirstName;
        registrationRequest.LastName = request.LastName;
        registrationRequest.Email = request.Email;
        registrationRequest.UserNameWithoutCompanyCode = request.UserNameWithoutCompanyCode;
        registrationRequest.CompanyCode = request.CompanyCode;
        registrationRequest.CompanyName = request.CompanyName;
        
        await MainDbContext.SaveChangesAsync(cancellationToken);

        await SendOkAsync(cancellationToken);
    }
}