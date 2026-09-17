using Microsoft.Extensions.Options;

namespace RaceDay.Infrastructure.Storage;

// Fallback storage used for local development when no Azure Blob Storage
// connection string is configured. Writes under wwwroot/uploads of the API
// host so files are served as static files. This satisfies "local
// development can use a safe development configuration if Azure
// credentials are unavailable" without faking the Azure integration.
public class LocalFileStorageService : IBlobStorageService
{
    private readonly StorageOptions _options;

    public LocalFileStorageService(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, string folder, CancellationToken ct = default)
    {
        var targetDir = Path.Combine(_options.LocalStoragePath, folder);
        Directory.CreateDirectory(targetDir);

        var safeName = $"{Guid.NewGuid():N}-{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(targetDir, safeName);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, ct);

        return $"{_options.LocalPublicBaseUrl}/{folder}/{safeName}";
    }

    public Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        var relative = fileUrl.Replace(_options.LocalPublicBaseUrl, string.Empty).TrimStart('/');
        var fullPath = Path.Combine(_options.LocalStoragePath, relative);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
