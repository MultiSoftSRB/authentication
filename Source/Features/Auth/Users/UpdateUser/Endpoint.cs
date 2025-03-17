using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Features.Auth.Users.UpdateUser;

sealed class Endpoint : Endpoint<Request>
{
    public MainDbContext MainDbContext { get; set; }
    public CompanyProvider CompanyProvider { get; set; }
    
    public override void Configure()
    {
        Put("auth/users/{id}");
        Permissions(ResourcePermissions.UserUpdate);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var companyId = CompanyProvider.GetCompanyId();
        var userId = Route<int>("id");

        // Check if user exists and belongs to the company
        var user = await MainDbContext.Users
            .Where(u => u.Id == userId)
            .Include(u => u.UserCompanies.Where(uc => uc.CompanyId == companyId))
            .Include(u => u.UserRoles.Where(ur => ur.CompanyId == companyId))
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null || user.UserCompanies.Count == 0)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        // Verify the role belongs to the company
        var roleExists = await MainDbContext.Roles
            .Where(r => r.CompanyId == companyId && r.Id == request.RoleId)
            .AnyAsync(cancellationToken);

        if (!roleExists)
            ThrowError("Invalid role");

        // Update user properties
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;

        // Update user role if different
        var currentUserRole = user.UserRoles.FirstOrDefault();
        if (currentUserRole == null)
        {
            // Add new role assignment
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = request.RoleId,
                CompanyId = companyId
            };
            MainDbContext.UserRoles.Add(userRole);
        }
        else if (currentUserRole.RoleId != request.RoleId)
        {
            // Update existing role
            currentUserRole.RoleId = request.RoleId;
        }

        // Save changes to the database
        await MainDbContext.SaveChangesAsync(cancellationToken);

        await SendOkAsync(cancellationToken);
    }
}