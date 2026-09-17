using Microsoft.EntityFrameworkCore;
using RaceDay.Api.DTOs;
using RaceDay.Domain.Entities;
using RaceDay.Infrastructure.Data;

namespace RaceDay.Api.Services;

public class CategoryService : ICategoryService
{
    private readonly RaceDayDbContext _db;
    public CategoryService(RaceDayDbContext db) => _db = db;

    public async Task<IReadOnlyList<CategoryResponse>> GetByEventAsync(Guid eventId, CancellationToken ct = default)
    {
        var categories = await _db.Categories.Where(c => c.EventId == eventId).OrderBy(c => c.Name).ToListAsync(ct);
        return categories.Select(ToResponse).ToList();
    }

    public async Task<CategoryResponse> CreateAsync(Guid eventId, Guid organiserId, CreateCategoryRequest request, CancellationToken ct = default)
    {
        var ev = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId, ct)
            ?? throw new NotFoundException("Event not found.");
        if (ev.OrganiserId != organiserId)
            throw new ForbiddenException("You can only manage categories for events that you organise.");

        var entity = new Category
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync(ct);
        return ToResponse(entity);
    }

    public async Task<CategoryResponse> UpdateAsync(Guid categoryId, Guid organiserId, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var entity = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.Id == categoryId, ct)
            ?? throw new NotFoundException("Category not found.");
        if (entity.Event!.OrganiserId != organiserId)
            throw new ForbiddenException("You can only manage categories for events that you organise.");

        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        await _db.SaveChangesAsync(ct);
        return ToResponse(entity);
    }

    public async Task DeleteAsync(Guid categoryId, Guid organiserId, CancellationToken ct = default)
    {
        var entity = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.Id == categoryId, ct)
            ?? throw new NotFoundException("Category not found.");
        if (entity.Event!.OrganiserId != organiserId)
            throw new ForbiddenException("You can only manage categories for events that you organise.");

        var hasEnrolments = await _db.Enrolments.AnyAsync(e => e.CategoryId == categoryId, ct);
        if (hasEnrolments)
            throw new ConflictException("This category has active enrolments and cannot be deleted.");

        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static CategoryResponse ToResponse(Category c) => new(c.Id, c.EventId, c.Name, c.Description, c.CreatedAt);
}
