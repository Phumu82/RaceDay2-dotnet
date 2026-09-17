using System.ComponentModel.DataAnnotations;
using RaceDay.Web.ApiClient.Models;

namespace RaceDay.Web.Models;

public class LoginViewModel
{
    [Required, EmailAddress, Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required, StringLength(200, MinimumLength = 2), Display(Name = "Full name")]
    public string FullName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
    // Mirrors the original app's isValidPassword(): 8+ chars, at least one letter and one digit.
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one letter and one number.")]
    public string Password { get; set; } = "";

    [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = "";

    [Required]
    public UserRole Role { get; set; } = UserRole.Participant;
}
