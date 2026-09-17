using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.Models;

public class ProfileViewModel
{
    public Guid Id { get; set; }

    [Required, StringLength(200, MinimumLength = 2), Display(Name = "Full name")]
    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public IFormFile? AvatarFile { get; set; }
}
