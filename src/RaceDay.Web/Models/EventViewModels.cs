using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using RaceDay.Web.ApiClient.Models;

namespace RaceDay.Web.Models;

public class EventBrowseViewModel
{
    public string? Search { get; set; }
    public EventType? Type { get; set; }
    public string? Location { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public List<EventResponse> Events { get; set; } = new();
}

public class EventFormViewModel
{
    public Guid? Id { get; set; }

    [Required, StringLength(200, MinimumLength = 3), Display(Name = "Event name")]
    public string Name { get; set; } = "";

    [StringLength(4000)]
    public string? Description { get; set; }

    [Required, DataType(DataType.Date), Display(Name = "Event date")]
    public DateOnly EventDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

    [Required, StringLength(300, MinimumLength = 2)]
    public string Location { get; set; } = "";

    [Required, Range(0.01, 10000), Display(Name = "Distance (km)")]
    public decimal DistanceKm { get; set; } = 5;

    [Required, Display(Name = "Event type")]
    public EventType EventType { get; set; } = EventType.Run;

    public string? BannerUrl { get; set; }
    public IFormFile? BannerFile { get; set; }
}
