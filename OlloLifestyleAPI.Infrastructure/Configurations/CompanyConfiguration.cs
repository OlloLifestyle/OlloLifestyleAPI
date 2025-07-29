using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OlloLifestyleAPI.Core.Entities.Master;

namespace OlloLifestyleAPI.Infrastructure.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.DatabaseName)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(e => e.ConnectionString)
               .IsRequired()
               .HasMaxLength(500);


        builder.Property(e => e.ContactEmail)
               .HasMaxLength(100);

        builder.Property(e => e.ContactPhone)
               .HasMaxLength(20);

        builder.Property(e => e.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
               .IsRequired()
               .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(e => e.Name)
               .IsUnique()
               .HasDatabaseName("IX_Companies_Name");

        builder.HasIndex(e => e.DatabaseName)
               .IsUnique()
               .HasDatabaseName("IX_Companies_DatabaseName");

    }
}