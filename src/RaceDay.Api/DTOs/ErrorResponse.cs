namespace RaceDay.Api.DTOs;

public record ErrorResponse(string Message, string? Detail = null, IDictionary<string, string[]>? Errors = null);
