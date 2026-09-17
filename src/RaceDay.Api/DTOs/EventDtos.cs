using System.ComponentModel.DataAnnotations;
using RaceDay.Domain.Enums;

namespace RaceDay.Api.DTOs;

public record EventResponse(
    Guid Id, Guid OrganiserId, string Name, string? Description, DateOnly EventDate,
    string Location, decimal DistanceKm, EventType EventType, string? BannerUrl,
    DateTime CreatedAt, ProfileSummary? Organiser, int CategoryCount, int EnrolmentCount);

public record CreateEventRequest(
    [property: Required, StringLength(200, MinimumLength = 3)] string Name,
    [property: StringLength(4000)] string? Description,
    [property: Required] DateOnly EventDate,
    [property: Required, StringLength(300, MinimumLength = 2)] string Location,
    [property: Required, Range(0.01, 10000)] decimal DistanceKm,
    [property: Required] EventType EventType,
    string? BannerUrl
);

public record UpdateEventRequest(
    [property: Required, StringLength(200, MinimumLength = 3)] string Name,
    [property: StringLength(4000)] string? Description,
    [property: Required] DateOnly EventDate,
    [property: Required, StringLength(300, MinimumLength = 2)] string Location,
    [property: Required, Range(0.01, 10000)] decimal DistanceKm,
    [property: Required] EventType EventType,
    string? BannerUrl
);

public record EventQuery(string? Search, EventType? Type, DateOnly? From, DateOnly? To, string? Location);
