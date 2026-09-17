using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.ApiClient.Models;

namespace RaceDay.Web.ApiClient;

// Thin typed wrapper around HttpClient. This is the ONLY way RaceDay.Web
// touches application data — it never references EF Core or a connection
// string, satisfying "MVC application must NOT directly access SQL Server".
public class RaceDayApiClient : IRaceDayApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public RaceDayApiClient(HttpClient http) => _http = http;

    private async Task<T> ReadOrThrowAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
            return result ?? throw new ApiException((int)response.StatusCode, "Empty response from API.");
        }

        string message = response.ReasonPhrase ?? "Request failed.";
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions, ct);
            if (error is not null) message = error.Message;
        }
        catch { /* body wasn't JSON — keep default message */ }

        throw new ApiException((int)response.StatusCode, message);
    }

    private async Task ThrowIfErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        string message = response.ReasonPhrase ?? "Request failed.";
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(JsonOptions, ct);
            if (error is not null) message = error.Message;
        }
        catch { }
        throw new ApiException((int)response.StatusCode, message);
    }

    public async Task<AuthResponse> RegisterAsync(string fullName, string email, string password, UserRole role, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", new { fullName, email, password, role }, JsonOptions, ct);
        return await ReadOrThrowAsync<AuthResponse>(response, ct);
    }

    public async Task<AuthResponse> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new { email, password }, JsonOptions, ct);
        return await ReadOrThrowAsync<AuthResponse>(response, ct);
    }

    public async Task<ProfileResponse> GetMyProfileAsync(CancellationToken ct = default)
        => await ReadOrThrowAsync<ProfileResponse>(await _http.GetAsync("api/users/profile", ct), ct);

    public async Task<ProfileResponse> UpdateMyProfileAsync(string fullName, CancellationToken ct = default)
        => await ReadOrThrowAsync<ProfileResponse>(await _http.PutAsJsonAsync("api/users/profile", new { fullName }, JsonOptions, ct), ct);

    public async Task<ProfileResponse> UploadAvatarAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);
        var response = await _http.PostAsync("api/media/avatar", content, ct);
        return await ReadOrThrowAsync<ProfileResponse>(response, ct);
    }

    public async Task<List<EventResponse>> SearchEventsAsync(string? search, EventType? type, string? location, DateOnly? from, DateOnly? to, CancellationToken ct = default)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
        if (type.HasValue) query.Add($"type={type}");
        if (!string.IsNullOrWhiteSpace(location)) query.Add($"location={Uri.EscapeDataString(location)}");
        if (from.HasValue) query.Add($"from={from:yyyy-MM-dd}");
        if (to.HasValue) query.Add($"to={to:yyyy-MM-dd}");
        var url = "api/events" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
        return await ReadOrThrowAsync<List<EventResponse>>(await _http.GetAsync(url, ct), ct);
    }

    public async Task<EventResponse> GetEventAsync(Guid id, CancellationToken ct = default)
        => await ReadOrThrowAsync<EventResponse>(await _http.GetAsync($"api/events/{id}", ct), ct);

    public async Task<List<EventResponse>> GetMyOrganisedEventsAsync(CancellationToken ct = default)
        => await ReadOrThrowAsync<List<EventResponse>>(await _http.GetAsync("api/events/mine", ct), ct);

    public async Task<EventResponse> CreateEventAsync(EventFormData data, CancellationToken ct = default)
        => await ReadOrThrowAsync<EventResponse>(await _http.PostAsJsonAsync("api/events", data, JsonOptions, ct), ct);

    public async Task<EventResponse> UpdateEventAsync(Guid id, EventFormData data, CancellationToken ct = default)
        => await ReadOrThrowAsync<EventResponse>(await _http.PutAsJsonAsync($"api/events/{id}", data, JsonOptions, ct), ct);

    public async Task DeleteEventAsync(Guid id, CancellationToken ct = default)
        => await ThrowIfErrorAsync(await _http.DeleteAsync($"api/events/{id}", ct), ct);

    public async Task<string> UploadEventBannerAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);
        var response = await _http.PostAsync("api/media/event-banner", content, ct);
        var result = await ReadOrThrowAsync<JsonElement>(response, ct);
        return result.GetProperty("url").GetString() ?? "";
    }

    public async Task<List<CategoryResponse>> GetCategoriesAsync(Guid eventId, CancellationToken ct = default)
        => await ReadOrThrowAsync<List<CategoryResponse>>(await _http.GetAsync($"api/events/{eventId}/categories", ct), ct);

    public async Task<CategoryResponse> CreateCategoryAsync(Guid eventId, string name, string? description, CancellationToken ct = default)
        => await ReadOrThrowAsync<CategoryResponse>(await _http.PostAsJsonAsync($"api/events/{eventId}/categories", new { name, description }, JsonOptions, ct), ct);

    public async Task<CategoryResponse> UpdateCategoryAsync(Guid id, string name, string? description, CancellationToken ct = default)
        => await ReadOrThrowAsync<CategoryResponse>(await _http.PutAsJsonAsync($"api/categories/{id}", new { name, description }, JsonOptions, ct), ct);

    public async Task DeleteCategoryAsync(Guid id, CancellationToken ct = default)
        => await ThrowIfErrorAsync(await _http.DeleteAsync($"api/categories/{id}", ct), ct);

    public async Task<EnrolmentResponse> EnrolAsync(Guid eventId, Guid categoryId, CancellationToken ct = default)
        => await ReadOrThrowAsync<EnrolmentResponse>(await _http.PostAsJsonAsync($"api/events/{eventId}/enrolments", new { categoryId }, JsonOptions, ct), ct);

    public async Task<List<EnrolmentResponse>> GetMyEnrolmentsAsync(CancellationToken ct = default)
        => await ReadOrThrowAsync<List<EnrolmentResponse>>(await _http.GetAsync("api/enrolments/my", ct), ct);

    public async Task<List<EnrolmentResponse>> GetEventEnrolmentsAsync(Guid eventId, CancellationToken ct = default)
        => await ReadOrThrowAsync<List<EnrolmentResponse>>(await _http.GetAsync($"api/events/{eventId}/enrolments", ct), ct);

    public async Task<ResultResponse> CreateResultAsync(Guid enrolmentId, TimeSpan? finishTime, int? position, CancellationToken ct = default)
        => await ReadOrThrowAsync<ResultResponse>(await _http.PostAsJsonAsync("api/results", new { enrolmentId, finishTime, position }, JsonOptions, ct), ct);

    public async Task<ResultResponse> UpdateResultAsync(Guid resultId, TimeSpan? finishTime, int? position, CancellationToken ct = default)
        => await ReadOrThrowAsync<ResultResponse>(await _http.PutAsJsonAsync($"api/results/{resultId}", new { finishTime, position }, JsonOptions, ct), ct);

    public async Task<List<ResultResponse>> GetMyResultsAsync(CancellationToken ct = default)
        => await ReadOrThrowAsync<List<ResultResponse>>(await _http.GetAsync("api/results/my", ct), ct);

    public async Task<List<ResultResponse>> GetEventResultsAsync(Guid eventId, CancellationToken ct = default)
        => await ReadOrThrowAsync<List<ResultResponse>>(await _http.GetAsync($"api/events/{eventId}/results", ct), ct);
}
