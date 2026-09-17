using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.ApiClient;
using RaceDay.Web.ApiClient.Models;

namespace RaceDay.Web.Controllers;

[Authorize(Roles = "Participant")]
public class ParticipantController : Controller
{
    private readonly IRaceDayApiClient _api;
    public ParticipantController(IRaceDayApiClient api) => _api = api;

    public async Task<IActionResult> Dashboard()
    {
        var enrolments = await _api.GetMyEnrolmentsAsync();
        var results = await _api.GetMyResultsAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        ViewBag.UpcomingCount = enrolments.Count(e => e.EventDate >= today);
        ViewBag.EnteredCount = enrolments.Count;
        ViewBag.CompletedCount = results.Count;
        ViewBag.PersonalBest = results.Where(r => r.FinishTime.HasValue).OrderBy(r => r.FinishTime).FirstOrDefault();
        ViewBag.NextRace = enrolments.Where(e => e.EventDate >= today).OrderBy(e => e.EventDate).FirstOrDefault();
        ViewBag.LatestResult = results.OrderByDescending(r => r.CreatedAt).FirstOrDefault();

        return View();
    }

    public async Task<IActionResult> MyEvents()
    {
        var enrolments = await _api.GetMyEnrolmentsAsync();
        return View(enrolments.OrderBy(e => e.EventDate).ToList());
    }

    public async Task<IActionResult> MyResults()
    {
        var results = await _api.GetMyResultsAsync();
        return View(results.OrderByDescending(r => r.EventDate).ToList());
    }
}
