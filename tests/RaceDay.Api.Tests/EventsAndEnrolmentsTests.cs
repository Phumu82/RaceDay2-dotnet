using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDay.Api.DTOs;
using RaceDay.Domain.Enums;
using Xunit;

namespace RaceDay.Api.Tests;

public class EventsAndEnrolmentsTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;
    public EventsAndEnrolmentsTests(RaceDayApiFactory factory) => _factory = factory;

    private async Task<(HttpClient client, EventResponse ev, CategoryResponse category)> CreateEventWithCategoryAsync()
    {
        var client = _factory.CreateClient();
        var (organiserToken, _) = await TestHelpers.RegisterAndLoginAsync(client, UserRole.Organiser);
        client.UseToken(organiserToken);

        var evResponse = await client.PostAsJsonAsync("/api/events", new CreateEventRequest(
            "5K Fun Run", "A fun run", DateOnly.FromDateTime(DateTime.Today.AddDays(15)), "Park", 5, EventType.Run, null));
        var ev = (await evResponse.Content.ReadFromJsonAsync<EventResponse>())!;

        var catResponse = await client.PostAsJsonAsync($"/api/events/{ev.Id}/categories", new CreateCategoryRequest("Open", null));
        var category = (await catResponse.Content.ReadFromJsonAsync<CategoryResponse>())!;

        return (client, ev, category);
    }

    [Fact]
    public async Task CreateReadUpdateDeleteEvent_Works()
    {
        var client = _factory.CreateClient();
        var (token, _) = await TestHelpers.RegisterAndLoginAsync(client, UserRole.Organiser);
        client.UseToken(token);

        var create = await client.PostAsJsonAsync("/api/events", new CreateEventRequest(
            "CRUD Event", null, DateOnly.FromDateTime(DateTime.Today.AddDays(5)), "Loc", 10, EventType.Cycle, null));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await create.Content.ReadFromJsonAsync<EventResponse>())!;

        var read = await client.GetAsync($"/api/events/{created.Id}");
        read.StatusCode.Should().Be(HttpStatusCode.OK);

        var update = await client.PutAsJsonAsync($"/api/events/{created.Id}", new UpdateEventRequest(
            "CRUD Event Updated", null, DateOnly.FromDateTime(DateTime.Today.AddDays(5)), "Loc", 12, EventType.Cycle, null));
        update.StatusCode.Should().Be(HttpStatusCode.OK);

        var delete = await client.DeleteAsync($"/api/events/{created.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var readAfterDelete = await client.GetAsync($"/api/events/{created.Id}");
        readAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Enrol_ThenPreventDuplicateEnrolment()
    {
        var (organiserClient, ev, category) = await CreateEventWithCategoryAsync();

        var participantClient = _factory.CreateClient();
        var (pToken, _) = await TestHelpers.RegisterAndLoginAsync(participantClient, UserRole.Participant);
        participantClient.UseToken(pToken);

        var first = await participantClient.PostAsJsonAsync($"/api/events/{ev.Id}/enrolments", new CreateEnrolmentRequest(category.Id));
        first.StatusCode.Should().Be(HttpStatusCode.Created);

        var duplicate = await participantClient.PostAsJsonAsync($"/api/events/{ev.Id}/enrolments", new CreateEnrolmentRequest(category.Id));
        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Participant_CanRetrieveTheirOwnEnrolments()
    {
        var (_, ev, category) = await CreateEventWithCategoryAsync();

        var participantClient = _factory.CreateClient();
        var (pToken, _) = await TestHelpers.RegisterAndLoginAsync(participantClient, UserRole.Participant);
        participantClient.UseToken(pToken);
        await participantClient.PostAsJsonAsync($"/api/events/{ev.Id}/enrolments", new CreateEnrolmentRequest(category.Id));

        var mine = await participantClient.GetFromJsonAsync<List<EnrolmentResponse>>("/api/enrolments/my");
        mine.Should().ContainSingle(e => e.EventId == ev.Id);
    }
}
