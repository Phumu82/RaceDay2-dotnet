using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDay.Api.DTOs;
using RaceDay.Domain.Enums;
using Xunit;

namespace RaceDay.Api.Tests;

public class ResultsTests : IClassFixture<RaceDayApiFactory>
{
    private readonly RaceDayApiFactory _factory;
    public ResultsTests(RaceDayApiFactory factory) => _factory = factory;

    private async Task<(HttpClient organiserClient, HttpClient participantClient, EnrolmentResponse enrolment)> SetupEnrolledParticipantAsync()
    {
        var organiserClient = _factory.CreateClient();
        var (organiserToken, _) = await TestHelpers.RegisterAndLoginAsync(organiserClient, UserRole.Organiser);
        organiserClient.UseToken(organiserToken);

        var evResponse = await organiserClient.PostAsJsonAsync("/api/events", new CreateEventRequest(
            "10K Race", null, DateOnly.FromDateTime(DateTime.Today.AddDays(-1)), "Track", 10, EventType.Run, null));
        var ev = (await evResponse.Content.ReadFromJsonAsync<EventResponse>())!;

        var catResponse = await organiserClient.PostAsJsonAsync($"/api/events/{ev.Id}/categories", new CreateCategoryRequest("Senior", null));
        var category = (await catResponse.Content.ReadFromJsonAsync<CategoryResponse>())!;

        var participantClient = _factory.CreateClient();
        var (pToken, _) = await TestHelpers.RegisterAndLoginAsync(participantClient, UserRole.Participant);
        participantClient.UseToken(pToken);

        var enrolResponse = await participantClient.PostAsJsonAsync($"/api/events/{ev.Id}/enrolments", new CreateEnrolmentRequest(category.Id));
        var enrolment = (await enrolResponse.Content.ReadFromJsonAsync<EnrolmentResponse>())!;

        return (organiserClient, participantClient, enrolment);
    }

    [Fact]
    public async Task Organiser_CanCreateAndUpdateResult()
    {
        var (organiserClient, _, enrolment) = await SetupEnrolledParticipantAsync();

        var create = await organiserClient.PostAsJsonAsync("/api/results",
            new CreateResultRequest(enrolment.Id, TimeSpan.FromMinutes(45), 1));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = (await create.Content.ReadFromJsonAsync<ResultResponse>())!;

        var update = await organiserClient.PutAsJsonAsync($"/api/results/{result.Id}",
            new UpdateResultRequest(TimeSpan.FromMinutes(44), 1));
        update.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Participant_CanRetrieveOwnResults()
    {
        var (organiserClient, participantClient, enrolment) = await SetupEnrolledParticipantAsync();
        await organiserClient.PostAsJsonAsync("/api/results", new CreateResultRequest(enrolment.Id, TimeSpan.FromMinutes(50), 3));

        var mine = await participantClient.GetFromJsonAsync<List<ResultResponse>>("/api/results/my");
        mine.Should().ContainSingle(r => r.EnrolmentId == enrolment.Id);
    }

    [Fact]
    public async Task Participant_CannotUpdateResults()
    {
        var (organiserClient, participantClient, enrolment) = await SetupEnrolledParticipantAsync();
        var create = await organiserClient.PostAsJsonAsync("/api/results", new CreateResultRequest(enrolment.Id, TimeSpan.FromMinutes(50), 3));
        var result = (await create.Content.ReadFromJsonAsync<ResultResponse>())!;

        var response = await participantClient.PutAsJsonAsync($"/api/results/{result.Id}", new UpdateResultRequest(TimeSpan.FromMinutes(40), 1));
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
