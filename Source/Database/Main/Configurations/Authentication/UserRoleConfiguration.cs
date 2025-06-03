using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Database.Main.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        
        builder.ToTable("UserRoles", schema: "ath");
        builder.HasKey(e => new { e.UserId, e.RoleId });
        
        builder.HasOne(e => e.User)
               .WithMany(u => u.UserRoles)
               .HasForeignKey(e => e.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Role)
               .WithMany(r => r.UserRoles)
               .HasForeignKey(e => e.RoleId)
               .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(e => e.Company)
               .WithMany()
               .HasForeignKey(e => e.CompanyId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}