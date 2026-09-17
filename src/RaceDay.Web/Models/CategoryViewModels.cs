using System.ComponentModel.DataAnnotations;
using RaceDay.Web.ApiClient.Models;

namespace RaceDay.Web.Models;

public class CategoryManagementViewModel
{
    public EventResponse Event { get; set; } = new();
    public List<CategoryResponse> Categories { get; set; } = new();
}

public class CategoryFormViewModel
{
    public Guid? Id { get; set; }
    public Guid EventId { get; set; }

    [Required, StringLength(150, MinimumLength = 1)]
    public string Name { get; set; } = "";

    [StringLength(1000)]
    public string? Description { get; set; }
}
