using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Database.Main.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(e => new { e.RoleId, e.PagePermissionCode });
        
        builder.HasOne(e => e.Role)
               .WithMany(r => r.Permissions)
               .HasForeignKey(e => e.RoleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}