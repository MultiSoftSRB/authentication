using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MultiSoftSRB.Entities.Audit;

namespace MultiSoftSRB.Database.Audit.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EntityName)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.ActionType)
            .IsRequired();

        builder.Property(e => e.Timestamp)
            .IsRequired()
            .HasConversion(
                new ValueConverter<DateTime, DateTime>(
                    v => v.ToUniversalTime(), // Convert to UTC before saving
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc) // Keep as UTC when reading
                ));

        builder.Property(e => e.UserName)
            .HasMaxLength(256);

        builder.Property(e => e.IsApiKeyAuth)
            .HasDefaultValue(false);
            
        builder.Property(e => e.EntityId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.Endpoint)
            .HasMaxLength(256);

        builder.Property(e => e.OldValues)
            .HasColumnType("jsonb");

        builder.Property(e => e.NewValues)
            .HasColumnType("jsonb");

        builder.Property(e => e.ChangedProperties)
            .HasColumnType("jsonb");

        builder.Property(e => e.ContextType)
            .HasMaxLength(128);

        // Indexes
        builder.HasIndex(e => e.EntityName);
        builder.HasIndex(e => e.CompanyId);
    }
}