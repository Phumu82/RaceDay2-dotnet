using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaceDay.Domain.Entities;

namespace RaceDay.Infrastructure.Data.Configurations;

public class ResultConfiguration : IEntityTypeConfiguration<Result>
{
    public void Configure(EntityTypeBuilder<Result> builder)
    {
        builder.ToTable("Results");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        builder.HasOne(r => r.Enrolment)
            .WithOne(e => e.Result)
            .HasForeignKey<Result>(r => r.EnrolmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.RecordedByProfile)
            .WithMany(p => p.ResultsRecorded)
            .HasForeignKey(r => r.RecordedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.EnrolmentId).IsUnique();
    }
}
