using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.ApiClient;
using RaceDay.Web.ApiClient.Models;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

[AllowAnonymous]
public class EventsController : Controller
{
    private readonly IRaceDayApiClient _api;
    public EventsController(IRaceDayApiClient api) => _api = api;

    // GET /Events?search=&type=&location=&from=&to=
    public async Task<IActionResult> Index(string? search, EventType? type, string? location, DateOnly? from, DateOnly? to)
    {
        var events = await _api.SearchEventsAsync(search, type, location, from, to);
        var vm = new EventBrowseViewModel { Search = search, Type = type, Location = location, From = from, To = to, Events = events };
        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var ev = await _api.GetEventAsync(id);
            ViewBag.Categories = await _api.GetCategoriesAsync(id);

            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Participant"))
            {
                var myEnrolments = await _api.GetMyEnrolmentsAsync();
                ViewBag.AlreadyEnrolled = myEnrolments.Any(e => e.EventId == id);
            }

            return View(ev);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return NotFound();
        }
    }

    [Authorize(Roles = "Participant")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(Guid eventId, Guid categoryId)
    {
        try
        {
            await _api.EnrolAsync(eventId, categoryId);
            TempData["Success"] = "You're enrolled! See it under My Events.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { id = eventId });
    }
}
