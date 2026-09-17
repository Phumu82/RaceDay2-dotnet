namespace RaceDay.Infrastructure.Storage;

public class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>"Azure" or "Local". Local is used automatically when no
    /// Azure connection string is configured, so the app runs in
    /// development without an Azure subscription.</summary>
    public string Provider { get; set; } = "Local";
    public string? AzureConnectionString { get; set; }
    public string ContainerName { get; set; } = "raceday-media";
    public string LocalStoragePath { get; set; } = "wwwroot/uploads";
    public string LocalPublicBaseUrl { get; set; } = "/uploads";
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024; // 5 MB
    public string[] AllowedContentTypes { get; set; } = { "image/jpeg", "image/png", "image/webp" };
}
