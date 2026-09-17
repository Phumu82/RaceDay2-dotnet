using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.DTOs;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    public CategoriesController(ICategoryService categoryService) => _categoryService = categoryService;

    [HttpGet("api/events/{eventId:guid}/categories")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetByEvent(Guid eventId, CancellationToken ct)
        => Ok(await _categoryService.GetByEventAsync(eventId, ct));

    [HttpPost("api/events/{eventId:guid}/categories")]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<CategoryResponse>> Create(Guid eventId, CreateCategoryRequest request, CancellationToken ct)
    {
        var result = await _categoryService.CreateAsync(eventId, CurrentUser.GetId(User), request, ct);
        return CreatedAtAction(nameof(GetByEvent), new { eventId }, result);
    }

    [HttpPut("api/categories/{id:guid}")]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<CategoryResponse>> Update(Guid id, UpdateCategoryRequest request, CancellationToken ct)
        => Ok(await _categoryService.UpdateAsync(id, CurrentUser.GetId(User), request, ct));

    [HttpDelete("api/categories/{id:guid}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _categoryService.DeleteAsync(id, CurrentUser.GetId(User), ct);
        return NoContent();
    }
}
