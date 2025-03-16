using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace MultiSoftSRB.Auth.Permissions;

sealed class UserPermissionClaimHydrator(UserProvider userProvider) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        long? userId = null;
        var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var uid))
            userId = uid;
        
        long? companyId = null;
        var companyIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "CompanyId");
        if (companyIdClaim != null && long.TryParse(companyIdClaim.Value, out var cid))
            companyId = cid;
        
        if (userId == null || companyId == null)
            return principal;
        
        var currentUserResourcePermissions = await userProvider.GetResourcePermissionsAsync(userId.Value, companyId.Value);
        if (currentUserResourcePermissions.Length != 0)
        {
            principal.AddIdentity(new(currentUserResourcePermissions.Select(p => new Claim(CustomClaimTypes.ResourcePermission, p))));
        }

        return principal;
    }
}