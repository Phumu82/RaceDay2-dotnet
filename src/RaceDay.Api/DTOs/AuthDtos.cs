using System.ComponentModel.DataAnnotations;
using RaceDay.Domain.Enums;

namespace RaceDay.Api.DTOs;

public record RegisterRequest(
    [property: Required, StringLength(200, MinimumLength = 2)] string FullName,
    [property: Required, EmailAddress] string Email,
    // Mirrors the original app's isValidPassword(): 8+ chars, at least one
    // letter and one digit.
    [property: Required, StringLength(100, MinimumLength = 8)]
    [property: RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one letter and one number.")]
    string Password,
    [property: Required] UserRole Role
);

public record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password
);

public record AuthResponse(string Token, DateTime ExpiresAt, ProfileResponse Profile);
