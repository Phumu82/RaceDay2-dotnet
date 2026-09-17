using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceDay.Domain.Entities;

namespace RaceDay.Infrastructure.Data.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(4000);
        builder.Property(e => e.Location).IsRequired().HasMaxLength(300);
        builder.Property(e => e.DistanceKm).HasColumnType("decimal(6,2)").IsRequired();
        builder.Property(e => e.EventType).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(e => e.BannerUrl).HasMaxLength(1000);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.ToTable(t => t.HasCheckConstraint("CK_Events_DistanceKm", "[DistanceKm] > 0"));

        builder.HasOne(e => e.Organiser)
            .WithMany(p => p.OrganisedEvents)
            .HasForeignKey(e => e.OrganiserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.OrganiserId);
        builder.HasIndex(e => e.EventDate);
        builder.HasIndex(e => e.EventType);
    }
}
