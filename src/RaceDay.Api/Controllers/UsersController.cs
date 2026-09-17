using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.DTOs;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IProfileService _profileService;
    public UsersController(IProfileService profileService) => _profileService = profileService;

    [HttpGet("profile")]
    public async Task<ActionResult<ProfileResponse>> GetProfile(CancellationToken ct)
        => Ok(await _profileService.GetAsync(CurrentUser.GetId(User), ct));

    [HttpPut("profile")]
    public async Task<ActionResult<ProfileResponse>> UpdateProfile(UpdateProfileRequest request, CancellationToken ct)
        => Ok(await _profileService.UpdateAsync(CurrentUser.GetId(User), request, ct));
}
