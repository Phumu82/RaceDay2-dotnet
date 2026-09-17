namespace RaceDay.Web.ApiClient.Models;

public enum UserRole { Participant, Organiser }
public enum EventType { Run, Walk, Cycle }
public enum EnrolmentStatus { Registered, Attended, Completed, Withdrawn }

public class ProfileResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public UserRole Role { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuthResponse
{
    public string Token { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public ProfileResponse Profile { get; set; } = new();
}

public class ProfileSummary
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
}

public class EventResponse
{
    public Guid Id { get; set; }
    public Guid OrganiserId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateOnly EventDate { get; set; }
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public EventType EventType { get; set; }
    public string? BannerUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public ProfileSummary? Organiser { get; set; }
    public int CategoryCount { get; set; }
    public int EnrolmentCount { get; set; }
}

public class CategoryResponse
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class EnrolmentResponse
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string EventName { get; set; } = "";
    public DateOnly EventDate { get; set; }
    public string Location { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public Guid ParticipantId { get; set; }
    public string ParticipantName { get; set; } = "";
    public string ParticipantEmail { get; set; } = "";
    public EnrolmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ResultResponse
{
    public Guid Id { get; set; }
    public Guid EnrolmentId { get; set; }
    public Guid EventId { get; set; }
    public string EventName { get; set; } = "";
    public DateOnly EventDate { get; set; }
    public string CategoryName { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public Guid ParticipantId { get; set; }
    public string ParticipantName { get; set; } = "";
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApiErrorResponse
{
    public string Message { get; set; } = "";
    public string? Detail { get; set; }
}
