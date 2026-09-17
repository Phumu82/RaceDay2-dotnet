using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceDay.Domain.Entities;

namespace RaceDay.Infrastructure.Data.Configurations;

public class EnrolmentConfiguration : IEntityTypeConfiguration<Enrolment>
{
    public void Configure(EntityTypeBuilder<Enrolment> builder)
    {
        builder.ToTable("Enrolments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(e => e.Event)
            .WithMany(ev => ev.Enrolments)
            .HasForeignKey(e => e.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Enrolments)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Participant)
            .WithMany(p => p.Enrolments)
            .HasForeignKey(e => e.ParticipantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Preserves the source schema's UNIQUE (event_id, participant_id) constraint
        // that prevents a participant enrolling twice in the same event.
        builder.HasIndex(e => new { e.EventId, e.ParticipantId }).IsUnique();
        builder.HasIndex(e => e.ParticipantId);
    }
}
