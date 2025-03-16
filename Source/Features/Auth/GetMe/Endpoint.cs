using MultiSoftSRB.Auth;
using MultiSoftSRB.Database.Main;

namespace MultiSoftSRB.Features.Auth.GetMe;

sealed class Endpoint : EndpointWithoutRequest<Response>
{
    public UserProvider UserProvider { get; set; }
    public MainDbContext MainDbContext { get; set; }
    
    public override void Configure()
    {
        Get("auth/me");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var currentUserId = UserProvider.GetCurrentUserId();
        var currentUser = (await MainDbContext.Users.FindAsync(currentUserId, cancellationToken))!;

        await SendOkAsync(new Response
        {
            Id = currentUser.Id,
            FirstName = currentUser.FirstName,
            LastName = currentUser.LastName,
            Email = currentUser.Email!,
            Username = currentUser.UserName!,
            PagePermissions = await UserProvider.GetCurrentUserPagePermissionsAsync(),
        }, cancellationToken);
    }
}