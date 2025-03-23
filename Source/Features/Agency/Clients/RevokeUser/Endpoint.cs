using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Agency.Clients.RevokeUser;

sealed class Endpoint : Endpoint<Request>
{
    public MainDbContext MainDbContext { get; set; }
    public UserProvider UserProvider { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    
    public override void Configure()
    {
        Delete("agency/companies/revoke-user");
        Permissions(ResourcePermissions.AgencyManage);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var agencyCompanyId = CompanyProvider.GetCompanyId();
        
        // Verify the company is managed by the agency
        var isAgencyClient = await MainDbContext.AgencyClients
            .AnyAsync(ac => ac.AgencyCompanyId == agencyCompanyId && ac.ClientCompanyId == request.CompanyId, cancellationToken);
        if (!isAgencyClient)
        {
            AddError("Company is not managed by this agency");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
        
        // Verify the user belongs to the agency
        var isAgencyUser = await MainDbContext.UserCompanies
            .AnyAsync(uc => uc.UserId == request.UserId && uc.CompanyId == agencyCompanyId, cancellationToken);
        if (!isAgencyUser)
        {
            AddError("User is not a member of the agency");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
        
        var userInCompany = await MainDbContext.UserCompanies.SingleOrDefaultAsync(uc => uc.UserId == request.UserId && uc.CompanyId == request.CompanyId, cancellationToken);
        // TODO: It might be usefull to combine UserRole into UserCompany entitiy, as it's just 1 field difference and adds overhead like this one
        var userRoleInCompany = await MainDbContext.UserRoles.SingleOrDefaultAsync(ur => ur.UserId == request.UserId && ur.CompanyId == request.CompanyId, cancellationToken);
        
        // Remove the consultant from this company
        if (userInCompany != null)
            MainDbContext.UserCompanies.Remove(userInCompany);
        if (userRoleInCompany != null)
            MainDbContext.UserRoles.Remove(userRoleInCompany);
        
        await SendOkAsync(cancellationToken);
    }
}