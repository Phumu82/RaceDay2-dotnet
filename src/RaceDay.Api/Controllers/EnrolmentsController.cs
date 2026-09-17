using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.DTOs;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly IEnrolmentService _enrolmentService;
    public EnrolmentsController(IEnrolmentService enrolmentService) => _enrolmentService = enrolmentService;

    [HttpPost("api/events/{eventId:guid}/enrolments")]
    [Authorize(Roles = "Participant")]
    public async Task<ActionResult<EnrolmentResponse>> Create(Guid eventId, CreateEnrolmentRequest request, CancellationToken ct)
    {
        var result = await _enrolmentService.CreateAsync(eventId, CurrentUser.GetId(User), request, ct);
        return CreatedAtAction(nameof(GetMine), null, result);
    }

    [HttpGet("api/enrolments/my")]
    [Authorize(Roles = "Participant")]
    public async Task<ActionResult<IReadOnlyList<EnrolmentResponse>>> GetMine(CancellationToken ct)
        => Ok(await _enrolmentService.GetMyEnrolmentsAsync(CurrentUser.GetId(User), ct));

    [HttpGet("api/events/{eventId:guid}/enrolments")]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<IReadOnlyList<EnrolmentResponse>>> GetByEvent(Guid eventId, CancellationToken ct)
        => Ok(await _enrolmentService.GetByEventAsync(eventId, CurrentUser.GetId(User), ct));
}
