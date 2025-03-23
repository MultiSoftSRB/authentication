using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Features.Admin.Consultants.AssignCompany;

sealed class Endpoint : Endpoint<Request>
{
    public MainDbContext MainDbContext { get; set; }
    
    public override void Configure()
    {
        Post("admin/consultants/companies");
        Permissions(ResourcePermissions.UserUpdate);
        Policies(CustomPolicies.SuperAdminOnly);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        // Validate consultant exists and is a consultant
        var consultant = await MainDbContext.Users
            .FirstOrDefaultAsync(u => u.Id == request.ConsultantUserId && u.UserType == UserType.Consultant, cancellationToken);
        
        if (consultant == null)
        {
            AddError("Consultant not found");
            await SendErrorsAsync(StatusCodes.Status404NotFound, cancellationToken);
            return;
        }
        
        // Validate company exists
        var companyExists = await MainDbContext.Companies.AnyAsync(c => c.Id == request.CompanyId, cancellationToken);
        if (!companyExists)
        {
            AddError("Company not found");
            await SendErrorsAsync(StatusCodes.Status404NotFound, cancellationToken);
            return;
        }
        
        // Check if relationship already exists
        var existingRelationship = await MainDbContext.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == request.ConsultantUserId && uc.CompanyId == request.CompanyId, cancellationToken);
            
        if (existingRelationship != null)
        {
            AddError("This consultant is already assigned to this company");
            await SendErrorsAsync(StatusCodes.Status409Conflict, cancellationToken);
            return;
        }
        
        // Create new user-company relationship
        var userCompany = new UserCompany
        {
            UserId = request.ConsultantUserId,
            CompanyId = request.CompanyId
        };
        
        await MainDbContext.UserCompanies.AddAsync(userCompany, cancellationToken);
        await MainDbContext.SaveChangesAsync(cancellationToken);
        
        await SendOkAsync(cancellationToken);
    }
}