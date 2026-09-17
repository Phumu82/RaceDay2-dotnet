using Microsoft.AspNetCore.Authentication.Cookies;
using RaceDay.Web.ApiClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthTokenHandler>();

// RaceDay.Web talks to RaceDay.Api exclusively over HTTP — it has no
// EF Core / SQL Server reference at all (see the csproj).
builder.Services.AddHttpClient<IRaceDayApiClient, RaceDayApiClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"] ?? throw new InvalidOperationException("Api:BaseUrl is not configured.");
    client.BaseAddress = new Uri(baseUrl);
}).AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Any 404 (unmatched route or explicit NotFound()) re-executes the
// pipeline against /not-found so the visitor gets the styled 404 page
// instead of a blank response — mirrors the original app's catch-all
// <Route path="*" element={<NotFoundPage />} />.
app.UseStatusCodePagesWithReExecute("/not-found");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
