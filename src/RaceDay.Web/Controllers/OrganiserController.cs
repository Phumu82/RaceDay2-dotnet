using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.ApiClient;
using RaceDay.Web.ApiClient.Models;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Organiser")]
public class OrganiserController : Controller
{
    private readonly IRaceDayApiClient _api;
    public OrganiserController(IRaceDayApiClient api) => _api = api;

    // ---------- Dashboard ----------
    public async Task<IActionResult> Dashboard()
    {
        var events = await _api.GetMyOrganisedEventsAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);

        ViewBag.TotalEvents = events.Count;
        ViewBag.UpcomingEvents = events.Count(e => e.EventDate >= today);
        ViewBag.CompletedEvents = events.Count(e => e.EventDate < today);
        ViewBag.TotalEnrolments = events.Sum(e => e.EnrolmentCount);
        ViewBag.Events = events.OrderByDescending(e => e.EventDate).ToList();

        return View();
    }

    // ---------- Events ----------
    public async Task<IActionResult> Events()
    {
        var events = await _api.GetMyOrganisedEventsAsync();
        return View(events.OrderByDescending(e => e.EventDate).ToList());
    }

    public IActionResult CreateEvent() => View("EventForm", new EventFormViewModel());

    public async Task<IActionResult> EditEvent(Guid id)
    {
        var ev = await _api.GetEventAsync(id);
        if (ev.OrganiserId != CurrentUserId()) return Forbid();

        var vm = new EventFormViewModel
        {
            Id = ev.Id, Name = ev.Name, Description = ev.Description, EventDate = ev.EventDate,
            Location = ev.Location, DistanceKm = ev.DistanceKm, EventType = ev.EventType, BannerUrl = ev.BannerUrl
        };
        return View("EventForm", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveEvent(EventFormViewModel model)
    {
        if (!ModelState.IsValid) return View("EventForm", model);

        try
        {
            var bannerUrl = model.BannerUrl;
            if (model.BannerFile is { Length: > 0 })
            {
                await using var stream = model.BannerFile.OpenReadStream();
                bannerUrl = await _api.UploadEventBannerAsync(stream, model.BannerFile.FileName, model.BannerFile.ContentType);
            }

            var data = new EventFormData(model.Name, model.Description, model.EventDate, model.Location, model.DistanceKm, model.EventType, bannerUrl);

            if (model.Id.HasValue)
            {
                await _api.UpdateEventAsync(model.Id.Value, data);
                TempData["Success"] = "Event updated.";
            }
            else
            {
                var created = await _api.CreateEventAsync(data);
                TempData["Success"] = "Event created. Now add categories.";
                return RedirectToAction(nameof(ManageCategories), new { eventId = created.Id });
            }
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("EventForm", model);
        }

        return RedirectToAction(nameof(Events));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        try
        {
            await _api.DeleteEventAsync(id);
            TempData["Success"] = "Event deleted.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Events));
    }

    public async Task<IActionResult> ManageEvent(Guid eventId)
    {
        var ev = await _api.GetEventAsync(eventId);
        var enrolments = await _api.GetEventEnrolmentsAsync(eventId);
        var results = await _api.GetEventResultsAsync(eventId);
        ViewBag.Enrolments = enrolments;
        ViewBag.Results = results;
        return View(ev);
    }

    // ---------- Categories ----------
    public async Task<IActionResult> ManageCategories(Guid eventId)
    {
        var ev = await _api.GetEventAsync(eventId);
        var categories = await _api.GetCategoriesAsync(eventId);
        return View(new CategoryManagementViewModel { Event = ev, Categories = categories });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(CategoryFormViewModel model)
    {
        try
        {
            if (model.Id.HasValue)
                await _api.UpdateCategoryAsync(model.Id.Value, model.Name, model.Description);
            else
                await _api.CreateCategoryAsync(model.EventId, model.Name, model.Description);
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(ManageCategories), new { eventId = model.EventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(Guid id, Guid eventId)
    {
        try
        {
            await _api.DeleteCategoryAsync(id);
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(ManageCategories), new { eventId });
    }

    // ---------- Enrolments ----------
    public async Task<IActionResult> Enrolments(Guid eventId)
    {
        var ev = await _api.GetEventAsync(eventId);
        var enrolments = await _api.GetEventEnrolmentsAsync(eventId);
        ViewBag.Event = ev;
        return View(enrolments);
    }

    // ---------- Results ----------
    public async Task<IActionResult> Results(Guid eventId)
    {
        var ev = await _api.GetEventAsync(eventId);
        var enrolments = await _api.GetEventEnrolmentsAsync(eventId);
        var results = await _api.GetEventResultsAsync(eventId);

        ViewBag.Event = ev;
        ViewBag.Results = results;
        // Enrolments that don't have a result yet, so the organiser can record one.
        ViewBag.PendingEnrolments = enrolments.Where(e => results.All(r => r.EnrolmentId != e.Id)).ToList();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordResult(Guid eventId, Guid enrolmentId, string? finishTime, int? position)
    {
        try
        {
            TimeSpan? ts = TimeSpan.TryParse(finishTime, out var parsed) ? parsed : null;
            await _api.CreateResultAsync(enrolmentId, ts, position);
            TempData["Success"] = "Result recorded.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Results), new { eventId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditResult(Guid eventId, Guid resultId, string? finishTime, int? position)
    {
        try
        {
            TimeSpan? ts = TimeSpan.TryParse(finishTime, out var parsed) ? parsed : null;
            await _api.UpdateResultAsync(resultId, ts, position);
            TempData["Success"] = "Result updated.";
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Results), new { eventId });
    }

    private Guid CurrentUserId() => Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
