using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDay.Api.DTOs;
using RaceDay.Domain.Enums;
using Xunit;

namespace RaceDay.Api.Tests;

public class AuthorizationTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;
    public AuthorizationTests(RaceDayApiFactory factory) => _factory = factory;

    [Fact]
    public async Task AnonymousUser_CannotAccessProtectedEndpoint()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/enrolments/my");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Participant_CannotCreateEvent()
    {
        var client = _factory.CreateClient();
        var (token, _) = await TestHelpers.RegisterAndLoginAsync(client, UserRole.Participant);
        client.UseToken(token);

        var response = await client.PostAsJsonAsync("/api/events", new CreateEventRequest(
            "Illegal Event", null, DateOnly.FromDateTime(DateTime.Today.AddDays(10)), "Nowhere", 5, RaceDay.Domain.Enums.EventType.Run, null));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Participant_CannotRecordResults()
    {
        var client = _factory.CreateClient();
        var (token, _) = await TestHelpers.RegisterAndLoginAsync(client, UserRole.Participant);
        client.UseToken(token);

        var response = await client.PostAsJsonAsync("/api/results", new CreateResultRequest(Guid.NewGuid(), null, 1));
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Organiser_CannotEditAnotherOrganisersEvent()
    {
        var client = _factory.CreateClient();

        var (ownerToken, _) = await TestHelpers.RegisterAndLoginAsync(client, UserRole.Organiser);
        client.UseToken(ownerToken);
        var createResponse = await client.PostAsJsonAsync("/api/events", new CreateEventRequest(
            "Owner's Event", null, DateOnly.FromDateTime(DateTime.Today.AddDays(20)), "City", 10, RaceDay.Domain.Enums.EventType.Run, null));
        var created = await createResponse.Content.ReadFromJsonAsync<EventResponse>();

        var (intruderToken, _) = await TestHelpers.RegisterAndLoginAsync(client, UserRole.Organiser);
        client.UseToken(intruderToken);

        var updateResponse = await client.PutAsJsonAsync($"/api/events/{created!.Id}", new UpdateEventRequest(
            "Hijacked", null, DateOnly.FromDateTime(DateTime.Today.AddDays(20)), "City", 10, RaceDay.Domain.Enums.EventType.Run, null));

        updateResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Organiser_CanManageTheirOwnEvent()
    {
        var client = _factory.CreateClient();
        var (token, _) = await TestHelpers.RegisterAndLoginAsync(client, UserRole.Organiser);
        client.UseToken(token);

        var createResponse = await client.PostAsJsonAsync("/api/events", new CreateEventRequest(
            "My Event", "desc", DateOnly.FromDateTime(DateTime.Today.AddDays(20)), "City", 10, RaceDay.Domain.Enums.EventType.Run, null));

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
