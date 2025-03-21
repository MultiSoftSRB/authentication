using Microsoft.AspNetCore.Identity;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Features.Admin.Consultants.CreateConsultant;

sealed class Endpoint : Endpoint<Request, Response>
{
    public MainDbContext MainDbContext { get; set; }
    public UserManager<User> UserManager { get; set; }
    
    public override void Configure()
    {
        Post("admin/consultants");
        Permissions(ResourcePermissions.UserCreate);
        Policies(CustomPolicies.SuperAdminOnly);
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        // Check if username is already in use
        var username = $"consultant.{request.UserName}";
        var usernameExists = await UserManager.FindByNameAsync(username);
        if (usernameExists != null)
        {
            AddError(r => r.Email, "A user with this username already exists");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }

        var consultant = new User
        {
            Email = request.Email,
            UserName = username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            UserType = UserType.Consultant
        };
            
        var result = await UserManager.CreateAsync(consultant, request.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                AddError(error.Description);
            
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
            
        var response = new Response
        {
            Id = consultant.Id
        };

        await SendOkAsync(response, cancellationToken);
    }
}