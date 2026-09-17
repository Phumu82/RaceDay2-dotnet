namespace RaceDay.Domain.Entities;

public class Result
{
    public Guid Id { get; set; }
    public Guid EnrolmentId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public Guid RecordedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Enrolment? Enrolment { get; set; }
    public Profile? RecordedByProfile { get; set; }
}
