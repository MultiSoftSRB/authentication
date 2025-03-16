using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Database.Main.Configurations;

public class UserCompanyConfiguration : IEntityTypeConfiguration<UserCompany>
{
    public void Configure(EntityTypeBuilder<UserCompany> builder)
    {
        builder.HasKey(e => new { e.UserId, e.CompanyId });
        
        builder.HasOne(e => e.User)
               .WithMany(u => u.UserCompanies)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Company)
               .WithMany(c => c.UserCompanies)
               .HasForeignKey(e => e.CompanyId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}