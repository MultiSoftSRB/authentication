using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Database.Main.Configurations;

public class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        
        builder.ToTable("Licenses", schema: "ath");
        builder.Property(e => e.Name).IsRequired().HasMaxLength(64);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Price).IsRequired().HasDefaultValue(0);
    }
}