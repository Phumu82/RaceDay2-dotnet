using RaceDay.Api.DTOs;

namespace RaceDay.Api.Services;

public interface IProfileService
{
    Task<ProfileResponse> GetAsync(Guid profileId, CancellationToken ct = default);
    Task<ProfileResponse> UpdateAsync(Guid profileId, UpdateProfileRequest request, CancellationToken ct = default);
    Task<ProfileResponse> UpdateAvatarAsync(Guid profileId, string avatarUrl, CancellationToken ct = default);
}
