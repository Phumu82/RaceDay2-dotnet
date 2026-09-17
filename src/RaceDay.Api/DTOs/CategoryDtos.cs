using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public record CategoryResponse(Guid Id, Guid EventId, string Name, string? Description, DateTime CreatedAt);

public record CreateCategoryRequest(
    [property: Required, StringLength(150, MinimumLength = 1)] string Name,
    [property: StringLength(1000)] string? Description
);

public record UpdateCategoryRequest(
    [property: Required, StringLength(150, MinimumLength = 1)] string Name,
    [property: StringLength(1000)] string? Description
);
