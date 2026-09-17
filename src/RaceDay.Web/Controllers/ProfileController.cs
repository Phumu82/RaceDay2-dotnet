using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.ApiClient;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly IRaceDayApiClient _api;
    public ProfileController(IRaceDayApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var profile = await _api.GetMyProfileAsync();
        return View(new ProfileViewModel
        {
            Id = profile.Id, FullName = profile.FullName, Email = profile.Email,
            Role = profile.Role.ToString(), AvatarUrl = profile.AvatarUrl, CreatedAt = profile.CreatedAt
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(nameof(Index), model);

        try
        {
            await _api.UpdateMyProfileAsync(model.FullName);

            if (model.AvatarFile is { Length: > 0 })
            {
                await using var stream = model.AvatarFile.OpenReadStream();
                await _api.UploadAvatarAsync(stream, model.AvatarFile.FileName, model.AvatarFile.ContentType);
            }

            TempData["Success"] = "Profile updated.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
}
