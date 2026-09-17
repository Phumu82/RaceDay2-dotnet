using RaceDay.Domain.Enums;

namespace RaceDay.Domain.Entities;

// Mirrors the existing Supabase `profiles` table: one row per user account,
// role stored directly on the profile (the source schema never split role
// into a separate table, so this migration preserves that design rather
// than introducing an unused join table).
public class Profile
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Authentication (replaces Supabase Auth)
    public string PasswordHash { get; set; } = string.Empty;
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");

    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    public ICollection<Result> ResultsRecorded { get; set; } = new List<Result>();
}
