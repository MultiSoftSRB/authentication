using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Features.Admin.Consultants.RevokeCompany;

sealed class Request
{
    public long ConsultantUserId { get; set; }
    public long CompanyId { get; set; }
}

sealed class Endpoint : Endpoint<Request>
{
    public MainDbContext MainDbContext { get; set; }
    
    public override void Configure()
    {
        Delete("admin/consultants/companies");
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
            
        // Find the user-company relationship
        var userCompany = await MainDbContext.UserCompanies
            .FirstOrDefaultAsync(uc => uc.UserId == request.ConsultantUserId && uc.CompanyId == request.CompanyId, cancellationToken);
        
        if (userCompany == null)
        {
            AddError("Consultant is not part of this company");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
        
        // TODO: It might be usefull to combine UserRole into UserCompany entitiy, as it's just 1 field difference and adds overhead like this one
        var userRoleInCompany = await MainDbContext.UserRoles.SingleOrDefaultAsync(ur => ur.UserId == request.ConsultantUserId && ur.CompanyId == request.CompanyId, cancellationToken);
        
        // Remove the consultant from this company
        consultant.LastUsedCompanyId = null;
        MainDbContext.UserCompanies.Remove(userCompany);
        if (userRoleInCompany != null)
            MainDbContext.UserRoles.Remove(userRoleInCompany);
            
        await MainDbContext.SaveChangesAsync(cancellationToken);
            
        await SendOkAsync(cancellationToken);
    }
}