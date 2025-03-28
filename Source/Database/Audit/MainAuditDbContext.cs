using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Entities.Audit;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Database.Audit;

public class MainAuditDbContext : BaseAuditDbContext
{
    public DbSet<CompanyAuditLog> Companies { get; set; }
    public DbSet<UserAuditLog> Users { get; set; }
    public DbSet<RoleAuditLog> Roles { get; set; }
    public DbSet<RolePermissionAuditLog> RolePermissions { get; set; }
    public DbSet<UserCompanyAuditLog> UserCompanies { get; set; }
    public DbSet<UserRoleAuditLog> UserRoles { get; set; }
    public DbSet<ApiKeyAuditLog> ApiKeys { get; set; }
    public DbSet<ApiKeyPermissionAuditLog> ApiKeyPermissions { get; set; }
    public DbSet<AgencyClientAuditLog> AgencyClients { get; set; }

    public MainAuditDbContext(DbContextOptions<MainAuditDbContext> options) : base(options) {}
}