using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RaceDay.Api.Services;
using RaceDay.Infrastructure.Storage;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IBlobStorageService _storage;
    private readonly IProfileService _profileService;
    private readonly StorageOptions _options;

    public MediaController(IBlobStorageService storage, IProfileService profileService, IOptions<StorageOptions> options)
    {
        _storage = storage;
        _profileService = profileService;
        _options = options.Value;
    }

    private void ValidateFile(IFormFile file)
    {
        if (file.Length == 0) throw new ValidationAppException("File is empty.");
        if (file.Length > _options.MaxFileSizeBytes) throw new ValidationAppException($"File exceeds the {_options.MaxFileSizeBytes / (1024 * 1024)}MB limit.");
        if (!_options.AllowedContentTypes.Contains(file.ContentType)) throw new ValidationAppException("Only JPEG, PNG or WEBP images are allowed.");
    }

    [HttpPost("avatar")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken ct)
    {
        ValidateFile(file);
        await using var stream = file.OpenReadStream();
        var url = await _storage.UploadAsync(stream, file.FileName, file.ContentType, "avatars", ct);
        var profile = await _profileService.UpdateAvatarAsync(CurrentUser.GetId(User), url, ct);
        return Ok(profile);
    }

    [HttpPost("event-banner")]
    [Authorize(Roles = "Organiser")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> UploadEventBanner(IFormFile file, CancellationToken ct)
    {
        ValidateFile(file);
        await using var stream = file.OpenReadStream();
        var url = await _storage.UploadAsync(stream, file.FileName, file.ContentType, "banners", ct);
        return Ok(new { url });
    }
}
