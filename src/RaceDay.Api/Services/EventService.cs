using Microsoft.EntityFrameworkCore;
using RaceDay.Api.DTOs;
using RaceDay.Domain.Entities;
using RaceDay.Infrastructure.Data;

namespace RaceDay.Api.Services;

public class EventService : IEventService
{
    private readonly RaceDayDbContext _db;
    public EventService(RaceDayDbContext db) => _db = db;

    public async Task<IReadOnlyList<EventResponse>> SearchAsync(EventQuery query, CancellationToken ct = default)
    {
        var q = _db.Events.Include(e => e.Organiser).Include(e => e.Categories).Include(e => e.Enrolments).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            q = q.Where(e => e.Name.ToLower().Contains(term) || (e.Description != null && e.Description.ToLower().Contains(term)));
        }
        if (query.Type.HasValue) q = q.Where(e => e.EventType == query.Type);
        if (!string.IsNullOrWhiteSpace(query.Location)) q = q.Where(e => e.Location.Contains(query.Location));
        if (query.From.HasValue) q = q.Where(e => e.EventDate >= query.From);
        if (query.To.HasValue) q = q.Where(e => e.EventDate <= query.To);

        var results = await q.OrderBy(e => e.EventDate).ToListAsync(ct);
        return results.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<EventResponse>> GetByOrganiserAsync(Guid organiserId, CancellationToken ct = default)
    {
        var events = await _db.Events.Include(e => e.Organiser).Include(e => e.Categories).Include(e => e.Enrolments)
            .Where(e => e.OrganiserId == organiserId).OrderByDescending(e => e.EventDate).ToListAsync(ct);
        return events.Select(ToResponse).ToList();
    }

    public async Task<EventResponse> GetAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _db.Events.Include(x => x.Organiser).Include(x => x.Categories).Include(x => x.Enrolments)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Event not found.");
        return ToResponse(e);
    }

    public async Task<EventResponse> CreateAsync(Guid organiserId, CreateEventRequest request, CancellationToken ct = default)
    {
        var entity = new Event
        {
            Id = Guid.NewGuid(),
            OrganiserId = organiserId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            EventDate = request.EventDate,
            Location = request.Location.Trim(),
            DistanceKm = request.DistanceKm,
            EventType = request.EventType,
            BannerUrl = request.BannerUrl,
            CreatedAt = DateTime.UtcNow
        };
        _db.Events.Add(entity);
        await _db.SaveChangesAsync(ct);
        return await GetAsync(entity.Id, ct);
    }

    public async Task<EventResponse> UpdateAsync(Guid id, Guid organiserId, UpdateEventRequest request, CancellationToken ct = default)
    {
        var entity = await _db.Events.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Event not found.");

        if (entity.OrganiserId != organiserId)
            throw new ForbiddenException("You can only manage events that you organise.");

        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.EventDate = request.EventDate;
        entity.Location = request.Location.Trim();
        entity.DistanceKm = request.DistanceKm;
        entity.EventType = request.EventType;
        entity.BannerUrl = request.BannerUrl;

        await _db.SaveChangesAsync(ct);
        return await GetAsync(entity.Id, ct);
    }

    public async Task DeleteAsync(Guid id, Guid organiserId, CancellationToken ct = default)
    {
        var entity = await _db.Events.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Event not found.");

        if (entity.OrganiserId != organiserId)
            throw new ForbiddenException("You can only manage events that you organise.");

        _db.Events.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static EventResponse ToResponse(Event e) => new(
        e.Id, e.OrganiserId, e.Name, e.Description, e.EventDate, e.Location, e.DistanceKm, e.EventType, e.BannerUrl,
        e.CreatedAt,
        e.Organiser is null ? null : new ProfileSummary(e.Organiser.Id, e.Organiser.FullName, e.Organiser.Email),
        e.Categories.Count, e.Enrolments.Count);
}
