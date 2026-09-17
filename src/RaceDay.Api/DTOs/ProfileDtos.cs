using System.ComponentModel.DataAnnotations;
using RaceDay.Domain.Enums;

namespace RaceDay.Api.DTOs;

public record ProfileResponse(Guid Id, string FullName, string Email, UserRole Role, string? AvatarUrl, DateTime CreatedAt);

public record UpdateProfileRequest(
    [property: Required, StringLength(200, MinimumLength = 2)] string FullName
);

public record ProfileSummary(Guid Id, string FullName, string Email);
