using System.Net.Http;
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
    private readonly ILogger<EventsController> _logger;
    public EventsController(IRaceDayApiClient api, ILogger<EventsController> logger)
    {
        _api = api;
        _logger = logger;
    }

    // GET /Events?search=&type=&location=&from=&to=
    public async Task<IActionResult> Index(string? search, EventType? type, string? location, DateOnly? from, DateOnly? to)
    {
        try
        {
            var events = await _api.SearchEventsAsync(search, type, location, from, to);
            var vm = new EventBrowseViewModel { Search = search, Type = type, Location = location, From = from, To = to, Events = events };
            return View(vm);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "RaceDay API is unreachable while loading Events page.");
            TempData["Error"] = "RaceDay services are temporarily unavailable. Please try again in a moment.";
            return View(new EventBrowseViewModel { Search = search, Type = type, Location = location, From = from, To = to });
        }
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
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "RaceDay API is unreachable while loading event details for {EventId}.", id);
            TempData["Error"] = "RaceDay services are temporarily unavailable. Please try again in a moment.";
            return RedirectToAction(nameof(Index));
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
