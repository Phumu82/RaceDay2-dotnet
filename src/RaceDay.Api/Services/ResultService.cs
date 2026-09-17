using Microsoft.EntityFrameworkCore;
using RaceDay.Api.DTOs;
using RaceDay.Domain.Entities;
using RaceDay.Domain.Enums;
using RaceDay.Infrastructure.Data;

namespace RaceDay.Api.Services;

public class ResultService : IResultService
{
    private readonly RaceDayDbContext _db;
    public ResultService(RaceDayDbContext db) => _db = db;

    public async Task<ResultResponse> CreateAsync(Guid organiserId, CreateResultRequest request, CancellationToken ct = default)
    {
        var enrolment = await _db.Enrolments.Include(e => e.Event).Include(e => e.Result)
            .FirstOrDefaultAsync(e => e.Id == request.EnrolmentId, ct)
            ?? throw new NotFoundException("Enrolment not found.");

        if (enrolment.Event!.OrganiserId != organiserId)
            throw new ForbiddenException("You can only record results for events that you organise.");

        if (enrolment.Result is not null)
            throw new ConflictException("A result has already been recorded for this enrolment. Use update instead.");

        var entity = new Result
        {
            Id = Guid.NewGuid(),
            EnrolmentId = enrolment.Id,
            FinishTime = request.FinishTime,
            Position = request.Position,
            RecordedBy = organiserId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Results.Add(entity);
        enrolment.Status = EnrolmentStatus.Completed;

        await _db.SaveChangesAsync(ct);
        return await LoadResponseAsync(entity.Id, ct);
    }

    public async Task<ResultResponse> UpdateAsync(Guid resultId, Guid organiserId, UpdateResultRequest request, CancellationToken ct = default)
    {
        var entity = await _db.Results.Include(r => r.Enrolment).ThenInclude(e => e!.Event)
            .FirstOrDefaultAsync(r => r.Id == resultId, ct)
            ?? throw new NotFoundException("Result not found.");

        if (entity.Enrolment!.Event!.OrganiserId != organiserId)
            throw new ForbiddenException("You can only edit results for events that you organise.");

        entity.FinishTime = request.FinishTime;
        entity.Position = request.Position;
        await _db.SaveChangesAsync(ct);
        return await LoadResponseAsync(entity.Id, ct);
    }

    public async Task<IReadOnlyList<ResultResponse>> GetMyResultsAsync(Guid participantId, CancellationToken ct = default)
    {
        var items = await Query().Where(r => r.Enrolment!.ParticipantId == participantId)
            .OrderByDescending(r => r.Enrolment!.Event!.EventDate).ToListAsync(ct);
        return items.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<ResultResponse>> GetByEventAsync(Guid eventId, Guid organiserId, CancellationToken ct = default)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct)
            ?? throw new NotFoundException("Event not found.");
        if (ev.OrganiserId != organiserId)
            throw new ForbiddenException("You can only view results for events that you organise.");

        var items = await Query().Where(r => r.Enrolment!.EventId == eventId)
            .OrderBy(r => r.Position ?? int.MaxValue).ToListAsync(ct);
        return items.Select(ToResponse).ToList();
    }

    private IQueryable<Result> Query() => _db.Results
        .Include(r => r.Enrolment).ThenInclude(e => e!.Event)
        .Include(r => r.Enrolment).ThenInclude(e => e!.Category)
        .Include(r => r.Enrolment).ThenInclude(e => e!.Participant);

    private async Task<ResultResponse> LoadResponseAsync(Guid id, CancellationToken ct)
    {
        var r = await Query().FirstAsync(x => x.Id == id, ct);
        return ToResponse(r);
    }

    private static ResultResponse ToResponse(Result r) => new(
        r.Id, r.EnrolmentId, r.Enrolment!.EventId, r.Enrolment.Event!.Name, r.Enrolment.Event.EventDate,
        r.Enrolment.Category!.Name, r.Enrolment.Event.DistanceKm, r.Enrolment.ParticipantId,
        r.Enrolment.Participant!.FullName, r.FinishTime, r.Position, r.CreatedAt);
}
