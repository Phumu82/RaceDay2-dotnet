using System.Net.Http.Headers;

namespace RaceDay.Web.ApiClient;

// Reads the JWT that RaceDay.Api issued at login (stored as a claim on the
// MVC app's own auth cookie) and attaches it as a Bearer token on every
// outgoing call to the API. This is how the MVC tier authenticates to the
// API tier instead of touching SQL Server or EF Core directly.
public class AuthTokenHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AuthTokenHandler(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value;
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
