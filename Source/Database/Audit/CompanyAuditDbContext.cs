using Microsoft.EntityFrameworkCore;
using MultiSoftSRB.Entities.Audit;
using MultiSoftSRB.Entities.Company;

namespace MultiSoftSRB.Database.Audit;

public class CompanyAuditDbContext : BaseAuditDbContext
{
    public DbSet<ArticleAuditLog> ArticleAuditLogs { get; set; }
    
    public CompanyAuditDbContext(DbContextOptions<CompanyAuditDbContext> options) : base(options) {}
}