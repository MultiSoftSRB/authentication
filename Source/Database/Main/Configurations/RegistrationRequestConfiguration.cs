using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Database.Main.Configurations;

public class RegistrationRequestConfiguration : IEntityTypeConfiguration<RegistrationRequest>
{
    public void Configure(EntityTypeBuilder<RegistrationRequest> builder)
    {
        builder.Property(e => e.UserNameWithoutCompanyCode).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(64);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(64);
        
        builder.Property(e => e.CompanyName).IsRequired().HasMaxLength(256);
        builder.Property(e => e.CompanyCode).IsRequired().HasMaxLength(20);
    }
}