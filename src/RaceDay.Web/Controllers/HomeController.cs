using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.ApiClient;
using RaceDay.Web.ApiClient.Models;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly IRaceDayApiClient _api;
    private readonly ILogger<HomeController> _logger;
    public HomeController(IRaceDayApiClient api, ILogger<HomeController> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var events = await _api.SearchEventsAsync(null, null, null, DateOnly.FromDateTime(DateTime.Today), null);
            return View(events.Take(6).ToList());
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "RaceDay API is unreachable while loading Home page.");
            TempData["Error"] = "RaceDay services are temporarily unavailable. Please try again in a moment.";
            return View(new List<EventResponse>());
        }
    }

    public IActionResult About() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }

    // Rendered for any unmatched route or explicit 404, via
    // UseStatusCodePagesWithReExecute in Program.cs (mirrors the original
    // React app's catch-all NotFoundPage route).
    [Route("/not-found")]
    public IActionResult NotFoundPage() => View("NotFound");
}
