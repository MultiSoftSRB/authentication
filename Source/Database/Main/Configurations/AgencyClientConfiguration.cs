using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Database.Main.Configurations;

public class AgencyClientConfiguration : IEntityTypeConfiguration<AgencyClient>
{
    public void Configure(EntityTypeBuilder<AgencyClient> builder)
    {
        builder.HasKey(e => new { e.AgencyCompanyId, e.ClientCompanyId });
                
        builder.HasOne(e => e.AgencyCompany)
            .WithMany()
            .HasForeignKey(e => e.AgencyCompanyId)
            .OnDelete(DeleteBehavior.Cascade);
                
        builder.HasOne(e => e.ClientCompany)
            .WithMany()
            .HasForeignKey(e => e.ClientCompanyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}