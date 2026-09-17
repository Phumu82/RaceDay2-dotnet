using Microsoft.EntityFrameworkCore;
using RaceDay.Api.DTOs;
using RaceDay.Domain.Entities;
using RaceDay.Domain.Enums;
using RaceDay.Infrastructure.Data;

namespace RaceDay.Api.Services;

public class EnrolmentService : IEnrolmentService
{
    private readonly RaceDayDbContext _db;
    public EnrolmentService(RaceDayDbContext db) => _db = db;

    public async Task<EnrolmentResponse> CreateAsync(Guid eventId, Guid participantId, CreateEnrolmentRequest request, CancellationToken ct = default)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct)
            ?? throw new NotFoundException("Event not found.");

        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.EventId == eventId, ct)
            ?? throw new NotFoundException("Category not found for this event.");

        // Mirrors the source schema's UNIQUE(event_id, participant_id) —
        // checked here for a clean 409 and enforced again at the DB via
        // the unique index configured in EnrolmentConfiguration.
        var alreadyEnrolled = await _db.Enrolments.AnyAsync(e => e.EventId == eventId && e.ParticipantId == participantId, ct);
        if (alreadyEnrolled)
            throw new ConflictException("You are already enrolled in this event.");

        var entity = new Enrolment
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            CategoryId = category.Id,
            ParticipantId = participantId,
            Status = EnrolmentStatus.Registered,
            CreatedAt = DateTime.UtcNow
        };

        _db.Enrolments.Add(entity);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException) // race condition caught by the unique index
        {
            throw new ConflictException("You are already enrolled in this event.");
        }

        return await LoadResponseAsync(entity.Id, ct);
    }

    public async Task<IReadOnlyList<EnrolmentResponse>> GetMyEnrolmentsAsync(Guid participantId, CancellationToken ct = default)
    {
        var items = await Query().Where(e => e.ParticipantId == participantId)
            .OrderByDescending(e => e.Event!.EventDate).ToListAsync(ct);
        return items.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<EnrolmentResponse>> GetByEventAsync(Guid eventId, Guid organiserId, CancellationToken ct = default)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct)
            ?? throw new NotFoundException("Event not found.");
        if (ev.OrganiserId != organiserId)
            throw new ForbiddenException("You can only view enrolments for events that you organise.");

        var items = await Query().Where(e => e.EventId == eventId).OrderBy(e => e.Participant!.FullName).ToListAsync(ct);
        return items.Select(ToResponse).ToList();
    }

    private IQueryable<Enrolment> Query() => _db.Enrolments
        .Include(e => e.Event).Include(e => e.Category).Include(e => e.Participant);

    private async Task<EnrolmentResponse> LoadResponseAsync(Guid id, CancellationToken ct)
    {
        var e = await Query().FirstAsync(x => x.Id == id, ct);
        return ToResponse(e);
    }

    private static EnrolmentResponse ToResponse(Enrolment e) => new(
        e.Id, e.EventId, e.Event!.Name, e.Event.EventDate, e.Event.Location, e.Event.DistanceKm,
        e.CategoryId, e.Category!.Name, e.ParticipantId, e.Participant!.FullName, e.Participant.Email,
        e.Status, e.CreatedAt);
}
