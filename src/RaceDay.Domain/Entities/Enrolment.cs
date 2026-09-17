using RaceDay.Domain.Enums;

namespace RaceDay.Domain.Entities;

public class Enrolment
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid ParticipantId { get; set; }
    public EnrolmentStatus Status { get; set; } = EnrolmentStatus.Registered;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Event? Event { get; set; }
    public Category? Category { get; set; }
    public Profile? Participant { get; set; }
    public Result? Result { get; set; }
}
