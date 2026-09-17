using RaceDay.Api.DTOs;

namespace RaceDay.Api.Services;

public interface IEventService
{
    Task<IReadOnlyList<EventResponse>> SearchAsync(EventQuery query, CancellationToken ct = default);
    Task<EventResponse> GetAsync(Guid id, CancellationToken ct = default);
    Task<EventResponse> CreateAsync(Guid organiserId, CreateEventRequest request, CancellationToken ct = default);
    Task<EventResponse> UpdateAsync(Guid id, Guid organiserId, UpdateEventRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid organiserId, CancellationToken ct = default);
    Task<IReadOnlyList<EventResponse>> GetByOrganiserAsync(Guid organiserId, CancellationToken ct = default);
}
