using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OlloLifestyleAPI.Core.Entities.Master;

namespace OlloLifestyleAPI.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(e => e.PasswordHash)
               .IsRequired()
               .HasMaxLength(256);

        builder.Property(e => e.FirstName)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(e => e.LastName)
               .IsRequired()
               .HasMaxLength(50);


        builder.Property(e => e.IsActive)
               .IsRequired()
               .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt)
               .IsRequired()
               .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(e => e.UserName)
               .IsUnique()
               .HasDatabaseName("IX_Users_UserName");
    }
}