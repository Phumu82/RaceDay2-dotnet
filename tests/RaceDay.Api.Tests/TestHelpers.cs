using System.Net.Http.Json;
using RaceDay.Api.DTOs;
using RaceDay.Domain.Enums;

namespace RaceDay.Api.Tests;

public static class TestHelpers
{
    public static async Task<(string Token, AuthResponse Auth)> RegisterAndLoginAsync(
        HttpClient client, UserRole role, string? email = null)
    {
        email ??= $"{Guid.NewGuid():N}@test.raceday.dev";
        var request = new RegisterRequest("Test User", email, "Password123!", role);
        var response = await client.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return (auth!.Token, auth);
    }

    public static void UseToken(this HttpClient client, string token)
        => client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
}
