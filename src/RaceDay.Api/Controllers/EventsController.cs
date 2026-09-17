using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.DTOs;
using RaceDay.Api.Services;
using RaceDay.Domain.Enums;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    public EventsController(IEventService eventService) => _eventService = eventService;

    // Public browsing — no auth required, matches the existing app where
    // anonymous visitors can browse events (RLS policy events_select_public).
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<EventResponse>>> Search(
        [FromQuery] string? search, [FromQuery] EventType? type, [FromQuery] string? location,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
        => Ok(await _eventService.SearchAsync(new EventQuery(search, type, from, to, location), ct));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<EventResponse>> GetById(Guid id, CancellationToken ct)
        => Ok(await _eventService.GetAsync(id, ct));

    [HttpGet("mine")]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<IReadOnlyList<EventResponse>>> GetMine(CancellationToken ct)
        => Ok(await _eventService.GetByOrganiserAsync(CurrentUser.GetId(User), ct));

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<EventResponse>> Create(CreateEventRequest request, CancellationToken ct)
    {
        var result = await _eventService.CreateAsync(CurrentUser.GetId(User), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<EventResponse>> Update(Guid id, UpdateEventRequest request, CancellationToken ct)
        => Ok(await _eventService.UpdateAsync(id, CurrentUser.GetId(User), request, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _eventService.DeleteAsync(id, CurrentUser.GetId(User), ct);
        return NoContent();
    }
}
