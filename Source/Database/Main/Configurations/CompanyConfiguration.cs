using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Database.Main.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Entities.Main.Company>
{
    public void Configure(EntityTypeBuilder<Entities.Main.Company> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(256);
        builder.Property(e => e.Code).HasMaxLength(20);
        builder.Property(e => e.LicenseCount).HasDefaultValue(1);
    }
}