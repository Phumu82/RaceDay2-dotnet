using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public record ResultResponse(
    Guid Id, Guid EnrolmentId, Guid EventId, string EventName, DateOnly EventDate, string CategoryName,
    decimal DistanceKm, Guid ParticipantId, string ParticipantName, TimeSpan? FinishTime, int? Position,
    DateTime CreatedAt);

public record CreateResultRequest(
    [property: Required] Guid EnrolmentId,
    TimeSpan? FinishTime,
    [property: Range(1, 100000)] int? Position
);

public record UpdateResultRequest(
    TimeSpan? FinishTime,
    [property: Range(1, 100000)] int? Position
);
