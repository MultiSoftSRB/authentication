using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using MultiSoftSRB.Auth.ApiKey;
using MultiSoftSRB.Entities.Main.Enums;

namespace MultiSoftSRB.Auth.Permissions;

sealed class UserPermissionClaimHydrator(UserProvider userProvider) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Check the authentication type and skip if using API key authentication
        var authenticationType = principal.Identity?.AuthenticationType;
        if (authenticationType == ApiKeyAuthenticationHandler.SchemeName)
            return principal;
        
        long? userId = null;
        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var uid))
            userId = uid;
        
        long? companyId = null;
        var companyIdClaim = principal.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.CompanyId);
        if (companyIdClaim != null && long.TryParse(companyIdClaim.Value, out var cid))
            companyId = cid;
        
        UserType? userType = null;
        var userTypeClaim = principal.Claims.FirstOrDefault(c => c.Type == CustomClaimTypes.UserType);
        if (userTypeClaim != null && Enum.TryParse<UserType>(userTypeClaim.Value, out var ut))
            userType = ut;
        
        if (userId == null)
            return principal;
        
        var currentUserResourcePermissions = await userProvider.GetResourcePermissionsAsync(userId.Value, companyId, userType);
        if (currentUserResourcePermissions.Length != 0)
        {
            principal.AddIdentity(new(currentUserResourcePermissions.Select(p => new Claim(CustomClaimTypes.ResourcePermission, p))));
        }

        return principal;
    }
}