using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace RaceDay.Infrastructure.Storage;

// Real Azure Blob Storage implementation. Used when Storage:Provider=Azure
// and Storage:AzureConnectionString is configured (appsettings / env var /
// Azure Key Vault / user-secrets — never hardcoded).
public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _container;
    private readonly StorageOptions _options;

    public AzureBlobStorageService(IOptions<StorageOptions> options)
    {
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.AzureConnectionString))
            throw new InvalidOperationException("Storage:AzureConnectionString is not configured.");

        var serviceClient = new BlobServiceClient(_options.AzureConnectionString);
        _container = serviceClient.GetBlobContainerClient(_options.ContainerName);
        _container.CreateIfNotExists(PublicAccessType.Blob);
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, string folder, CancellationToken ct = default)
    {
        var blobName = $"{folder}/{Guid.NewGuid():N}-{Path.GetFileName(fileName)}";
        var blobClient = _container.GetBlobClient(blobName);

        await blobClient.UploadAsync(content, new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        }, ct);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        var uri = new Uri(fileUrl);
        var blobName = uri.AbsolutePath.Substring(uri.AbsolutePath.IndexOf(_options.ContainerName, StringComparison.OrdinalIgnoreCase) + _options.ContainerName.Length + 1);
        var blobClient = _container.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
    }
}
