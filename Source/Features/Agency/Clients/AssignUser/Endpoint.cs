using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Features.Agency.Clients.AssignUser;

sealed class Endpoint : Endpoint<Request>
{
    public MainDbContext MainDbContext { get; set; }
    public UserProvider UserProvider { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    
    public override void Configure()
    {
        Post("agency/companies/assign-user");
        Permissions(ResourcePermissions.AgencyManage);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var agencyCompanyId = CompanyProvider.GetCompanyId();
        
        // Verify the user belongs to the agency
        var isAgencyUser = await MainDbContext.UserCompanies
            .AnyAsync(uc => uc.UserId == request.UserId && uc.CompanyId == agencyCompanyId, cancellationToken);
        if (!isAgencyUser)
        {
            AddError("User is not a member of the agency");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
                
        // Verify the company is managed by the agency
        var isAgencyClient = await MainDbContext.AgencyClients
            .AnyAsync(ac => ac.AgencyCompanyId == agencyCompanyId && ac.ClientCompanyId == request.CompanyId, cancellationToken);
        if (!isAgencyClient)
        {
            AddError("Company is not managed by this agency");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
                
        // Check if user exists
        var userExists = await MainDbContext.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            AddError("User not found");
            await SendErrorsAsync(StatusCodes.Status404NotFound, cancellationToken);
            return;
        }
                
        // Check if company exists
        var companyExists = await MainDbContext.Companies.AnyAsync(c => c.Id == request.CompanyId, cancellationToken);
        if (!companyExists)
        {
            AddError("Company not found");
            await SendErrorsAsync(StatusCodes.Status404NotFound, cancellationToken);
            return;
        }
                
        // Check if role exists
        var roleExists = await MainDbContext.Roles.AnyAsync(r => r.Id == request.RoleId && r.CompanyId == request.CompanyId, cancellationToken);
        if (!roleExists)
        {
            AddError("Role not found");
            await SendErrorsAsync(StatusCodes.Status404NotFound, cancellationToken);
            return;
        }
                
        // Check if user already has access to the company
        var hasExistingAccess = await MainDbContext.UserCompanies
            .AnyAsync(uc => uc.UserId == request.UserId && uc.CompanyId == request.CompanyId, cancellationToken);
        
        if (hasExistingAccess)
        {
            AddError("User is already member of the client company");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }

        // Create new user-company association
        var userCompany = new UserCompany
        {
            UserId = request.UserId,
            CompanyId = request.CompanyId,
            IsPrimary = false,
            AccessType = AccessType.AgencyAccess,
            AgencyCompanyId = agencyCompanyId
        };
            
        await MainDbContext.UserCompanies.AddAsync(userCompany, cancellationToken);
        
        // Assign role to user
        var userRole = new UserRole
        {
            UserId = request.UserId,
            RoleId = request.RoleId,
            CompanyId = request.CompanyId
        };
        
        await MainDbContext.UserRoles.AddAsync(userRole, cancellationToken);
        await MainDbContext.SaveChangesAsync(cancellationToken);
                
        // Clear permission cache for the user
        // This is important so the user gets their updated permissions
        UserProvider.ClearPermissionCache(request.UserId, request.CompanyId);
        
        await SendOkAsync(cancellationToken);
    }
}