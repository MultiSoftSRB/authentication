using Microsoft.AspNetCore.Identity;
using MultiSoftSRB.Auth;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Features.Auth.ChangePassword;

sealed class Endpoint : Endpoint<Request>
{
    public MainDbContext MainDbContext { get; set; }
    public UserProvider UserProvider { get; set; }
    public UserManager<User> UserManager { get; set; }
    
    public override void Configure()
    {
        Post("auth/change-password");
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var currentUserId = UserProvider.GetCurrentUserId();
        var currentUser = (await MainDbContext.Users.FindAsync(currentUserId, cancellationToken))!;
        
        // Check if current password is correct
        var isCurrentPasswordValid = await UserManager.CheckPasswordAsync(currentUser, request.CurrentPassword);
        if (!isCurrentPasswordValid)
        {
            AddError(r => r.CurrentPassword, "Current password is incorrect");
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
            return;
        }
        
        // Change the password
        var result = await UserManager.ChangePasswordAsync(currentUser, request.CurrentPassword, request.NewPassword);
        if (result.Succeeded)
        {
            await UserManager.UpdateSecurityStampAsync(currentUser);
            await SendOkAsync(cancellationToken);
        }
        else
        {
            // Add identity errors
            foreach (var error in result.Errors)
                AddError(error.Description);
            
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellationToken);
        }
    }
}