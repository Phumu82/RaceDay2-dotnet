using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceDay.Domain.Entities;

namespace RaceDay.Infrastructure.Data.Configurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ToTable("Profiles");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.FullName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(p => p.Email).IsUnique();

        builder.Property(p => p.Role).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.AvatarUrl).HasMaxLength(1000);
        builder.Property(p => p.PasswordHash).IsRequired();
        builder.Property(p => p.SecurityStamp).IsRequired();
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
    }
}
