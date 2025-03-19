using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MultiSoftSRB.Entities.Main;

namespace MultiSoftSRB.Database.Main.Configurations;

public class ApiKeyPermissionConfiguration: IEntityTypeConfiguration<ApiKeyPermission>
{
    public void Configure(EntityTypeBuilder<ApiKeyPermission> builder)
    {
        builder.HasKey(e => new { e.ApiKeyId, e.ResourcePermissionCode });
                
        builder.HasOne(e => e.ApiKey)
            .WithMany(k => k.Permissions)
            .HasForeignKey(e => e.ApiKeyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}