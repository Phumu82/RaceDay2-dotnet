using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.DTOs;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/results")]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly IResultService _resultService;
    public ResultsController(IResultService resultService) => _resultService = resultService;

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<ResultResponse>> Create(CreateResultRequest request, CancellationToken ct)
    {
        var result = await _resultService.CreateAsync(CurrentUser.GetId(User), request, ct);
        return CreatedAtAction(nameof(GetMine), null, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<ResultResponse>> Update(Guid id, UpdateResultRequest request, CancellationToken ct)
        => Ok(await _resultService.UpdateAsync(id, CurrentUser.GetId(User), request, ct));

    [HttpGet("my")]
    [Authorize(Roles = "Participant")]
    public async Task<ActionResult<IReadOnlyList<ResultResponse>>> GetMine(CancellationToken ct)
        => Ok(await _resultService.GetMyResultsAsync(CurrentUser.GetId(User), ct));

    [HttpGet("~/api/events/{eventId:guid}/results")]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<IReadOnlyList<ResultResponse>>> GetByEvent(Guid eventId, CancellationToken ct)
        => Ok(await _resultService.GetByEventAsync(eventId, CurrentUser.GetId(User), ct));
}
