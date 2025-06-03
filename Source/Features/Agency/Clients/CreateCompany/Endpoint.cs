using System.Transactions;
using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;
using MultiSoftSRB.Entities.Main.Enums;
using MultiSoftSRB.Services;

namespace MultiSoftSRB.Features.Agency.Clients.CreateCompany;

sealed class Endpoint : Endpoint<Request, Response>
{
    public MainDbContext MainDbContext { get; set; }
    public UserProvider UserProvider { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    public RolesService RolesService { get; set; }
    
    public override void Configure()
    {
        Post("agency/companies");
        Permissions(ResourcePermissions.CompanyCreate);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var currentUserId = UserProvider.GetCurrentUserId();
        var currentUsersCompanyId = CompanyProvider.GetCompanyId();
        
        // Check if company code already exists
        var existingCompany = await MainDbContext.Companies.AnyAsync(c => c.Code == request.CompanyCode, cancellationToken);
        if (existingCompany)
        {
            AddError("Company code already in use");
            await SendErrorsAsync(StatusCodes.Status409Conflict, cancellationToken);
            return;
        }
        
        await using var transaction = await MainDbContext.Database.BeginTransactionAsync(cancellationToken);        try
        {
            // Create company
            var clientCompany = new Company
            {
                Name = request.CompanyName,
                Code = request.CompanyCode,
            };
            
            await MainDbContext.Companies.AddAsync(clientCompany, cancellationToken);
            await MainDbContext.SaveChangesAsync(cancellationToken);
            
            // Associate user with company
            var userCompany = new UserCompany
            {
                UserId = currentUserId,
                CompanyId = clientCompany.Id,
                AgencyCompanyId = currentUsersCompanyId,
                AccessType = AccessType.AgencyAccess
            };
            
            await MainDbContext.UserCompanies.AddAsync(userCompany, cancellationToken);
            
            // Create default roles
            var roles = RolesService.GenerateDefaultRoles(clientCompany.Id);
            await MainDbContext.Roles.AddRangeAsync(roles, cancellationToken);
            await MainDbContext.SaveChangesAsync(cancellationToken);
            
            // Assign admin role to user
            var adminRole = roles.FirstOrDefault()!;
            var userRole = new UserRole
            {
                UserId = currentUserId,
                RoleId = adminRole.Id,
                CompanyId = clientCompany.Id
            };
            
            await MainDbContext.UserRoles.AddAsync(userRole, cancellationToken);
            await MainDbContext.SaveChangesAsync(cancellationToken);

            var agencyClient = new AgencyClient
            {
                AgencyCompanyId = currentUsersCompanyId,
                ClientCompanyId = clientCompany.Id
            };
            await MainDbContext.AgencyClients.AddAsync(agencyClient, cancellationToken);
            await MainDbContext.SaveChangesAsync(cancellationToken);
            
            // After all the changes, now we commit the transaction
            await transaction.CommitAsync(cancellationToken);
            
            await SendOkAsync(new Response
            {
                CompanyId = clientCompany.Id
            }, cancellation: cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            AddError("An error occurred during registration");
            await SendErrorsAsync(StatusCodes.Status500InternalServerError, cancellationToken);
        }
    }
}