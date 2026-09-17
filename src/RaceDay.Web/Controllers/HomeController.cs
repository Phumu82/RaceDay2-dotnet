using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.ApiClient;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

[AllowAnonymous]
public class HomeController : Controller
{
    private readonly IRaceDayApiClient _api;
    public HomeController(IRaceDayApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var events = await _api.SearchEventsAsync(null, null, null, DateOnly.FromDateTime(DateTime.Today), null);
        return View(events.Take(6).ToList());
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
