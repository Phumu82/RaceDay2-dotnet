using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Auth;
using RaceDay.Api.DTOs;
using RaceDay.Domain.Entities;
using RaceDay.Infrastructure.Data;

namespace RaceDay.Api.Services;

public class AuthService : IAuthService
{
    private readonly RaceDayDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher<Profile> _passwordHasher;

    public AuthService(RaceDayDbContext db, ITokenService tokenService, IPasswordHasher<Profile> passwordHasher)
    {
        _db = db;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var exists = await _db.Profiles.AnyAsync(p => p.Email == email, ct);
        if (exists)
            throw new ConflictException("An account with this email already exists.");

        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };
        profile.PasswordHash = _passwordHasher.HashPassword(profile, request.Password);

        _db.Profiles.Add(profile);
        await _db.SaveChangesAsync(ct);

        var (token, expires) = _tokenService.CreateToken(profile);
        return new AuthResponse(token, expires, ToResponse(profile));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.Email == email, ct);

        // Same generic failure message whether the email or password was
        // wrong, so we never confirm which emails have accounts.
        if (profile is null)
            throw new ValidationAppException("Invalid email or password.");

        var result = _passwordHasher.VerifyHashedPassword(profile, profile.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new ValidationAppException("Invalid email or password.");

        var (token, expires) = _tokenService.CreateToken(profile);
        return new AuthResponse(token, expires, ToResponse(profile));
    }

    private static ProfileResponse ToResponse(Profile p) =>
        new(p.Id, p.FullName, p.Email, p.Role, p.AvatarUrl, p.CreatedAt);
}
