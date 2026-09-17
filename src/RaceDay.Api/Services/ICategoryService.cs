using RaceDay.Api.DTOs;

namespace RaceDay.Api.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetByEventAsync(Guid eventId, CancellationToken ct = default);
    Task<CategoryResponse> CreateAsync(Guid eventId, Guid organiserId, CreateCategoryRequest request, CancellationToken ct = default);
    Task<CategoryResponse> UpdateAsync(Guid categoryId, Guid organiserId, UpdateCategoryRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid categoryId, Guid organiserId, CancellationToken ct = default);
}
