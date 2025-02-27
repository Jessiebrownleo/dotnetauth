using System.ComponentModel.DataAnnotations;

namespace DotnetAuthentication.Models.DTOs;

public class LoginRequest
{
    [Required]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Description = "The email address associated with the account.")]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    [Display(Description = "The password used for authentication.")]
    public required string Password { get; set; }
}