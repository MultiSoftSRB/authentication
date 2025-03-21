using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MultiSoftSRB.Auth.Permissions;
using MultiSoftSRB.Database.Main;
using MultiSoftSRB.Entities.Main.Enums;
using ZiggyCreatures.Caching.Fusion;

namespace MultiSoftSRB.Auth;

public class UserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly MainDbContext _mainDbContext;
    private readonly IFusionCache _cache;
    
    private const string PagePermissionCachePrefix = "PagePermissions_";
    private const string ResourcePermissionCachePrefix = "ResourcePermissions_";
    
    public UserProvider(IHttpContextAccessor httpContextAccessor, MainDbContext mainDbContext, IFusionCache cache)
    {
        _httpContextAccessor = httpContextAccessor;
        _mainDbContext = mainDbContext;
        _cache = cache;
    }
    
    public long GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && long.TryParse(userIdClaim.Value, out var userId)) 
            return userId;
        
        ValidationContext.Instance.ThrowError("User ID is not present in the token.", StatusCodes.Status401Unauthorized);
        return default;
    }
    
    public long? GetCurrentCompanyId()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(CustomClaimTypes.CompanyId);
        return claim != null && long.TryParse(claim.Value, out var companyId) ? companyId : null;
    }
    
    public UserType? GetCurrentUserType()
    {
        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(CustomClaimTypes.UserType);
        return claim != null && Enum.TryParse<UserType>(claim.Value, out var userType) ? userType : null;
    }
    
    public async Task<string[]> GetCurrentUserPagePermissionsAsync()
    {
        var userId = GetCurrentUserId();
        var companyId = GetCurrentCompanyId();
        
        if (!companyId.HasValue)
            return [];
            
        return await GetPagePermissionsAsync(userId, companyId.Value);
    }
    
    public async Task<string[]> GetCurrentUserResourcePermissionsAsync()
    {
        var userId = GetCurrentUserId();
        var companyId = GetCurrentCompanyId();
        
        if (!companyId.HasValue)
            return [];
            
        return await GetResourcePermissionsAsync(userId, companyId.Value);
    }
    
    public async Task<string[]> GetPagePermissionsAsync(long userId, long? companyId)
    {
        // Generate cache key
        var cacheKey = $"{PagePermissionCachePrefix}{userId}_{companyId}";
        
        // Setup FusionCache options
        var options = new FusionCacheEntryOptions
        {
            Duration = TimeSpan.FromMinutes(15),
            Priority = CacheItemPriority.High
        };
        
        // Get from cache or compute if not found
        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ => await FetchPagePermissionsAsync(userId, companyId),
            options);
    }
    
    private async Task<string[]> FetchPagePermissionsAsync(long userId, long? companyId)
    {
        // SuperAdmin and TenantAdmin have all permissions
        var userType = GetCurrentUserType();
        if (userType is not UserType.TenantUser)
            return PagePermissions.GetAll().ToArray();

        if (companyId == null)
            return [];
        
        // Get user roles for the company
        var roleIds = await _mainDbContext.UserRoles
            .Where(ur => ur.UserId == userId && ur.CompanyId == companyId)
            .Select(ur => ur.RoleId)
            .ToListAsync();
        
        if (!roleIds.Any())
            return [];
        
        // Get role permissions
        var permissions = await _mainDbContext.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PagePermissionCode)
            .Distinct()
            .ToArrayAsync();
        
        return permissions;
    }
    
    public async Task<string[]> GetResourcePermissionsAsync(long userId, long? companyId, UserType? userType = null)
    {
        // Generate cache key
        var cacheKey = $"{ResourcePermissionCachePrefix}{userId}_{companyId}";
        
        // Setup FusionCache options
        var options = new FusionCacheEntryOptions
        {
            Duration = TimeSpan.FromMinutes(15),
            Priority = CacheItemPriority.High
        };
        
        // Get from cache or compute if not found
        return await _cache.GetOrSetAsync(
            cacheKey,
            async _ => await FetchResourcePermissionsAsync(userId, companyId, userType),
            options);
    }
    
    private async Task<string[]> FetchResourcePermissionsAsync(long userId, long? companyId, UserType? userType = null)
    {
        // SuperAdmin and TenantAdmin have all permissions
        userType ??= GetCurrentUserType();
        if (userType is not UserType.TenantUser)
            return ResourcePermissions.GetAll().ToArray();
        
        if (companyId == null)
            return [];
        
        // Get page permissions first
        var pagePermissions = await GetPagePermissionsAsync(userId, companyId);
        
        // Map to resource permissions using static mapper
        var resourcePermissions = PermissionMapper.MapToResourcePermissions(pagePermissions);
        
        return resourcePermissions;
    }
    
    public bool HasResourcePermission(string permission)
    {
        // Get permission claims
        var claims = _httpContextAccessor.HttpContext?.User?.FindAll(CustomClaimTypes.ResourcePermission).ToList();
        if (claims == null || claims.Count == 0)
            return false;
        
        // Check if user has the permission
        return claims.Any(c => c.Value == permission);
    }
    
    public void ClearCurrentUserPermissionCache()
    {
        var userId = GetCurrentUserId();
        var companyId = GetCurrentCompanyId();
        
        if (companyId.HasValue)
            ClearPermissionCache(userId, companyId.Value);
    }
    
    public void ClearPermissionCache(long userId, long companyId)
    {
        var pagePermissionCacheKey = $"{PagePermissionCachePrefix}{userId}_{companyId}";
        var resourcePermissionCacheKey = $"{ResourcePermissionCachePrefix}{userId}_{companyId}";
        
        _cache.Remove(pagePermissionCacheKey);
        _cache.Remove(resourcePermissionCacheKey);
    }
}