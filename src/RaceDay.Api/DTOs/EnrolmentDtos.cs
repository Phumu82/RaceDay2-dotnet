using System.ComponentModel.DataAnnotations;
using RaceDay.Domain.Enums;

namespace RaceDay.Api.DTOs;

public record EnrolmentResponse(
    Guid Id, Guid EventId, string EventName, DateOnly EventDate, string Location, decimal DistanceKm,
    Guid CategoryId, string CategoryName, Guid ParticipantId, string ParticipantName, string ParticipantEmail,
    EnrolmentStatus Status, DateTime CreatedAt);

public record CreateEnrolmentRequest([property: Required] Guid CategoryId);

public record UpdateEnrolmentStatusRequest([property: Required] EnrolmentStatus Status);
