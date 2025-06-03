using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Database.Main.Configurations;

public class MunicipalityConfiguration : IEntityTypeConfiguration<Municipality>
{
    public void Configure(EntityTypeBuilder<Municipality> builder)
    {
        builder.ToTable("Municipalities", schema: "cnf");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.MunicipalityCode)
            .HasMaxLength(10);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        // Foreign key with cascade delete
        builder.HasOne(x => x.Region)
            .WithMany(m => m.Municipalities )
            .HasForeignKey(x => x.RegionId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}