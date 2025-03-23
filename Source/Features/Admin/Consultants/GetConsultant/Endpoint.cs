using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Features.Admin.Consultants.GetConsultant;

sealed class Endpoint : EndpointWithoutRequest<Response>
{
    public MainDbContext MainDbContext { get; set; }
    
    public override void Configure()
    {
        Get("admin/consultants/{id}");
        Permissions(ResourcePermissions.UserRead);
        Policies(CustomPolicies.SuperAdminOnly);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var consultantId = Route<long>("id");
        var consultant = await MainDbContext.Users
            .Include(u => u.UserCompanies)
                .ThenInclude(uc => uc.Company)
            .Where(u => u.Id == consultantId && u.UserType == UserType.Consultant)
            .FirstOrDefaultAsync(cancellationToken);
                
        if (consultant == null)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }
            
        var response = new Response
        {
            Id = consultant.Id,
            UserName = consultant.UserName,
            Email = consultant.Email,
            FullName = $"{consultant.FirstName} {consultant.LastName}",
            PhoneNumber = consultant.PhoneNumber,
            Companies = consultant.UserCompanies
                .Select(uc => new Response.ConsultantCompanyDto
                {
                    Id = uc.CompanyId,
                    Name = uc.Company.Name
                })
                .ToList()
        };
            
        await SendOkAsync(response, cancellationToken);
    }
}