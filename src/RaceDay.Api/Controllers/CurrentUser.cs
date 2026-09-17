using System.Security.Claims;

namespace RaceDay.Api.Controllers;

public static class CurrentUser
{
    public static Guid GetId(ClaimsPrincipal user)
    {
        var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Missing user id claim.");
        return Guid.Parse(idClaim);
    }
}
