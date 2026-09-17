using RaceDay.Api.DTOs;

namespace RaceDay.Api.Services;

public interface IResultService
{
    Task<ResultResponse> CreateAsync(Guid organiserId, CreateResultRequest request, CancellationToken ct = default);
    Task<ResultResponse> UpdateAsync(Guid resultId, Guid organiserId, UpdateResultRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ResultResponse>> GetMyResultsAsync(Guid participantId, CancellationToken ct = default);
    Task<IReadOnlyList<ResultResponse>> GetByEventAsync(Guid eventId, Guid organiserId, CancellationToken ct = default);
}
