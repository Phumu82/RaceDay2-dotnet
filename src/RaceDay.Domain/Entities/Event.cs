using RaceDay.Domain.Enums;

namespace RaceDay.Domain.Entities;

public class Event
{
    public Guid Id { get; set; }
    public Guid OrganiserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public EventType EventType { get; set; }
    public string? BannerUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Profile? Organiser { get; set; }
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
