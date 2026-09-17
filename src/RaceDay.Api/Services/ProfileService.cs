using Microsoft.EntityFrameworkCore;
using RaceDay.Api.DTOs;
using RaceDay.Infrastructure.Data;

namespace RaceDay.Api.Services;

public class ProfileService : IProfileService
{
    private readonly RaceDayDbContext _db;
    public ProfileService(RaceDayDbContext db) => _db = db;

    public async Task<ProfileResponse> GetAsync(Guid profileId, CancellationToken ct = default)
    {
        var p = await _db.Profiles.FindAsync([profileId], ct)
            ?? throw new NotFoundException("Profile not found.");
        return new ProfileResponse(p.Id, p.FullName, p.Email, p.Role, p.AvatarUrl, p.CreatedAt);
    }

    public async Task<ProfileResponse> UpdateAsync(Guid profileId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        var p = await _db.Profiles.FindAsync([profileId], ct)
            ?? throw new NotFoundException("Profile not found.");
        p.FullName = request.FullName.Trim();
        await _db.SaveChangesAsync(ct);
        return new ProfileResponse(p.Id, p.FullName, p.Email, p.Role, p.AvatarUrl, p.CreatedAt);
    }

    public async Task<ProfileResponse> UpdateAvatarAsync(Guid profileId, string avatarUrl, CancellationToken ct = default)
    {
        var p = await _db.Profiles.FindAsync([profileId], ct)
            ?? throw new NotFoundException("Profile not found.");
        p.AvatarUrl = avatarUrl;
        await _db.SaveChangesAsync(ct);
        return new ProfileResponse(p.Id, p.FullName, p.Email, p.Role, p.AvatarUrl, p.CreatedAt);
    }
}
