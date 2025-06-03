using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Database.Main.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries", schema: "cnf");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.IsoCode)
            .IsRequired()
            .HasMaxLength(2);

        builder.HasIndex(x => x.IsoCode).IsUnique();

        builder.Property(x => x.Iso3Code)
            .HasMaxLength(3);

        builder.HasIndex(x => x.Iso3Code).IsUnique();

        builder.HasIndex(x => x.NumericCode).IsUnique();

        builder.Property(x => x.PhoneCode)
            .HasMaxLength(5);

        builder.Property(x => x.Capital)
            .HasMaxLength(100);

        builder.Property(x => x.Currency)
            .HasMaxLength(3);

        builder.Property(x => x.CurrencySymbol)
            .HasMaxLength(5);

        builder.Property(x => x.Tld)
            .HasMaxLength(5);

        builder.Property(x => x.NativeName)
            .HasMaxLength(100);

        builder.Property(x => x.Latitude);

        builder.Property(x => x.Longitude);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);
    }
}