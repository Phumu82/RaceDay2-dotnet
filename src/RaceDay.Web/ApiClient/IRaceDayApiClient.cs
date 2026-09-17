using RaceDay.Web.ApiClient.Models;

namespace RaceDay.Web.ApiClient;

public interface IRaceDayApiClient
{
    // Auth
    Task<AuthResponse> RegisterAsync(string fullName, string email, string password, UserRole role, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(string email, string password, CancellationToken ct = default);

    // Profile
    Task<ProfileResponse> GetMyProfileAsync(CancellationToken ct = default);
    Task<ProfileResponse> UpdateMyProfileAsync(string fullName, CancellationToken ct = default);
    Task<ProfileResponse> UploadAvatarAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default);

    // Events
    Task<List<EventResponse>> SearchEventsAsync(string? search, EventType? type, string? location, DateOnly? from, DateOnly? to, CancellationToken ct = default);
    Task<EventResponse> GetEventAsync(Guid id, CancellationToken ct = default);
    Task<List<EventResponse>> GetMyOrganisedEventsAsync(CancellationToken ct = default);
    Task<EventResponse> CreateEventAsync(EventFormData data, CancellationToken ct = default);
    Task<EventResponse> UpdateEventAsync(Guid id, EventFormData data, CancellationToken ct = default);
    Task DeleteEventAsync(Guid id, CancellationToken ct = default);
    Task<string> UploadEventBannerAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default);

    // Categories
    Task<List<CategoryResponse>> GetCategoriesAsync(Guid eventId, CancellationToken ct = default);
    Task<CategoryResponse> CreateCategoryAsync(Guid eventId, string name, string? description, CancellationToken ct = default);
    Task<CategoryResponse> UpdateCategoryAsync(Guid id, string name, string? description, CancellationToken ct = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken ct = default);

    // Enrolments
    Task<EnrolmentResponse> EnrolAsync(Guid eventId, Guid categoryId, CancellationToken ct = default);
    Task<List<EnrolmentResponse>> GetMyEnrolmentsAsync(CancellationToken ct = default);
    Task<List<EnrolmentResponse>> GetEventEnrolmentsAsync(Guid eventId, CancellationToken ct = default);

    // Results
    Task<ResultResponse> CreateResultAsync(Guid enrolmentId, TimeSpan? finishTime, int? position, CancellationToken ct = default);
    Task<ResultResponse> UpdateResultAsync(Guid resultId, TimeSpan? finishTime, int? position, CancellationToken ct = default);
    Task<List<ResultResponse>> GetMyResultsAsync(CancellationToken ct = default);
    Task<List<ResultResponse>> GetEventResultsAsync(Guid eventId, CancellationToken ct = default);
}

public record EventFormData(string Name, string? Description, DateOnly EventDate, string Location, decimal DistanceKm, EventType EventType, string? BannerUrl);
