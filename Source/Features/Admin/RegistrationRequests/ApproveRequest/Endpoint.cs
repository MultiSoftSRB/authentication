using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;
using MultiSoftSRB.Entities.Main.Enums;
using MultiSoftSRB.Services;

namespace MultiSoftSRB.Features.Admin.RegistrationRequests.ApproveRequest;

sealed class Endpoint : EndpointWithoutRequest
{
    public UserManager<User> UserManager { get; set; }
    public MainDbContext MainDbContext { get; set; }
    public TokenService TokenService { get; set; }
    public RolesService RolesService { get; set; }
    
    public override void Configure()
    {
        Post("admin/registration-requests/{id}/approve");
        Policies(CustomPolicies.SuperAdminOnly);
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
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
        
        // Check if email already exists
        var existingUser = await UserManager.FindByEmailAsync(registrationRequest.Email);
        if (existingUser != null)
        {
            AddError("Email already in use");
            await SendErrorsAsync(StatusCodes.Status409Conflict, cancellationToken);
            return;
        }
        
        // Check if company code already exists
        var existingCompany = await MainDbContext.Companies.AnyAsync(c => c.Code == registrationRequest.CompanyCode, cancellationToken);
        if (existingCompany)
        {
            AddError("Company code already in use");
            await SendErrorsAsync(StatusCodes.Status409Conflict, cancellationToken);
            return;
        }
        
        await using var transaction = await MainDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Create company
            var company = new Company
            {
                Name = registrationRequest.CompanyName,
                Code = registrationRequest.CompanyCode
            };
            
            await MainDbContext.Companies.AddAsync(company, cancellationToken);
            await MainDbContext.SaveChangesAsync(cancellationToken);
            
            // Create user
            var user = new User
            {
                UserName = $"{company.Code}.{registrationRequest.UserNameWithoutCompanyCode}",
                NormalizedUserName = $"{company.Code}.{registrationRequest.UserNameWithoutCompanyCode}".ToUpper(),
                Email = registrationRequest.Email,
                NormalizedEmail = registrationRequest.Email.ToUpper(),
                FirstName = registrationRequest.FirstName,
                LastName = registrationRequest.LastName,
                UserType = UserType.TenantAdmin,
                LastUsedCompanyId = company.Id
            };
            
            var createUserResult = await UserManager.CreateAsync(user, registrationRequest.Password);
            if (!createUserResult.Succeeded)
            {
                var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                
                AddError(errors);
                await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
                return;
            }
            
            // Associate user with company
            var userCompany = new UserCompany
            {
                UserId = user.Id,
                CompanyId = company.Id,
                IsPrimary = true
            };
            
            await MainDbContext.UserCompanies.AddAsync(userCompany, cancellationToken);
            
            // Create default roles
            var roles = RolesService.GenerateDefaultRoles(company.Id);
            await MainDbContext.Roles.AddRangeAsync(roles, cancellationToken);
            await MainDbContext.SaveChangesAsync(cancellationToken);
            
            // Assign admin role to user
            var adminRole = roles.FirstOrDefault()!;
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = adminRole.Id,
                CompanyId = company.Id
            };
            
            await MainDbContext.UserRoles.AddAsync(userRole, cancellationToken);
            await MainDbContext.SaveChangesAsync(cancellationToken);

            // At the end, we remove the registration request, as we approved it and created company from it
            MainDbContext.Remove(registrationRequest);
            
            // After all the changes, now we commit the transaction
            await transaction.CommitAsync(cancellationToken);
            
            await SendOkAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            AddError("An error occurred during registration");
            await SendErrorsAsync(StatusCodes.Status500InternalServerError, cancellationToken);
        }
    }
}