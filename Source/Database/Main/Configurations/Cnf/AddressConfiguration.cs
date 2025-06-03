using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Database.Main.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    
    
    
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses", schema: "cnf");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Street)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.StreetNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Apartment)
            .HasMaxLength(20);

        builder.Property(x => x.Description)
            .HasMaxLength(200);

        builder.Property(x => x.IsPrimary)
            .HasDefaultValue(true);

        builder.HasOne(x => x.Settlement)
            .WithMany(a => a.Addresses)
            .HasForeignKey(x => x.SettlementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}