namespace RaceDay.Infrastructure.Storage;

public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a file to the configured container/folder and returns the
    /// publicly accessible URL to store on the entity (Event.BannerUrl,
    /// Profile.AvatarUrl).
    /// </summary>
    Task<string> UploadAsync(Stream content, string fileName, string contentType, string folder, CancellationToken ct = default);

    Task DeleteAsync(string fileUrl, CancellationToken ct = default);
}
