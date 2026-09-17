using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.ApiClient;
using RaceDay.Web.ApiClient.Models;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

public class AccountController : Controller
{
    private readonly IRaceDayApiClient _api;
    public AccountController(IRaceDayApiClient api) => _api = api;

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var auth = await _api.LoginAsync(model.Email, model.Password);
            await SignInAsync(auth);
            return RedirectToLocal(model.ReturnUrl, auth.Profile.Role);
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var auth = await _api.RegisterAsync(model.FullName, model.Email, model.Password, model.Role);
            await SignInAsync(auth);
            return RedirectToLocal(null, auth.Profile.Role);
        }
        catch (ApiException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private async Task SignInAsync(AuthResponse auth)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, auth.Profile.Id.ToString()),
            new(ClaimTypes.Name, auth.Profile.FullName),
            new(ClaimTypes.Email, auth.Profile.Email),
            new(ClaimTypes.Role, auth.Profile.Role.ToString()),
            new("access_token", auth.Token)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity),
            new AuthenticationProperties { ExpiresUtc = auth.ExpiresAt, IsPersistent = true });
    }

    private IActionResult RedirectToLocal(string? returnUrl, UserRole role)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return role == UserRole.Organiser
            ? RedirectToAction("Dashboard", "Organiser")
            : RedirectToAction("Dashboard", "Participant");
    }
}
