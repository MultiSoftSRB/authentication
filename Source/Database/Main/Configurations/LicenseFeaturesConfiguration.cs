using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Database.Main.Configurations;

public class LicenseFeaturesConfiguration : IEntityTypeConfiguration<LicenseFeature>
{
    public void Configure(EntityTypeBuilder<LicenseFeature> builder)
    {
        builder.HasKey(e => new { e.LicenseId, e.FeaturePermissionCode });
        
        builder.HasOne(e => e.License)
               .WithMany(r => r.Features)
               .HasForeignKey(e => e.LicenseId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}