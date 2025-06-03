using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Features.Auth.Register;

sealed class Endpoint : Endpoint<Request>
{
    public MainDbContext MainDbContext { get; set; }
    
    public override void Configure()
    {
        Post("auth/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var registrationRequest = new RegistrationRequest
        {
            Timestamp = DateTime.Now,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserNameWithoutCompanyCode = request.UserNameWithoutCompanyCode,
            Password = request.Password,
            CompanyCode = request.CompanyCode,
            CompanyName = request.CompanyName
        };
        
        await MainDbContext.RegistrationRequests.AddAsync(registrationRequest, cancellationToken);
        await MainDbContext.SaveChangesAsync(cancellationToken);

        await SendOkAsync(cancellationToken);
    }
}