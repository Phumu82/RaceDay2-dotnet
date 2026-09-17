using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models;

public class RecordResultViewModel
{
    [Required]
    public Guid EnrolmentId { get; set; }
    public Guid ResultId { get; set; }
    public string ParticipantName { get; set; } = "";
    public string CategoryName { get; set; } = "";

    [Display(Name = "Finish time (hh:mm:ss)")]
    public string? FinishTime { get; set; }

    [Range(1, 100000)]
    public int? Position { get; set; }
}
