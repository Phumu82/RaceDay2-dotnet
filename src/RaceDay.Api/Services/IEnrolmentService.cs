using RaceDay.Api.DTOs;

namespace RaceDay.Api.Services;

public interface IEnrolmentService
{
    Task<EnrolmentResponse> CreateAsync(Guid eventId, Guid participantId, CreateEnrolmentRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<EnrolmentResponse>> GetMyEnrolmentsAsync(Guid participantId, CancellationToken ct = default);
    Task<IReadOnlyList<EnrolmentResponse>> GetByEventAsync(Guid eventId, Guid organiserId, CancellationToken ct = default);
}
