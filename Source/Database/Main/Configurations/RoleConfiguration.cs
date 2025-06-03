using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Database.Main.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        
        builder.ToTable("Roles", schema: "ath");
        builder.HasOne(e => e.Company)
              .WithMany(c => c.Roles)
              .HasForeignKey(e => e.CompanyId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}