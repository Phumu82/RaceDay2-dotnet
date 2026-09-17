using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDay.Api.DTOs;
using RaceDay.Domain.Enums;
using Xunit;

namespace RaceDay.Api.Tests;

public class AuthenticationTests : IClassFixture<RaceDayApiFactory>
{
    private readonly HttpClient _client;
    public AuthenticationTests(RaceDayApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithValidData_ReturnsTokenAndProfile()
    {
        var (_, auth) = await TestHelpers.RegisterAndLoginAsync(_client, UserRole.Participant);
        auth.Profile.Role.Should().Be(UserRole.Participant);
        auth.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        var email = $"{Guid.NewGuid():N}@test.raceday.dev";
        await TestHelpers.RegisterAndLoginAsync(_client, UserRole.Participant, email);

        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Another Name", email, "Password123!", UserRole.Participant));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var email = $"{Guid.NewGuid():N}@test.raceday.dev";
        await TestHelpers.RegisterAndLoginAsync(_client, UserRole.Participant, email);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password123!"));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest($"{Guid.NewGuid():N}@nowhere.dev", "WrongPassword1!"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
