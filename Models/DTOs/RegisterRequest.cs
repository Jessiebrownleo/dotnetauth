using System.ComponentModel.DataAnnotations;

namespace DotnetAuthentication.Models.DTOs;

public class RegisterRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters.")]
    public required string Username { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
    public required string Password { get; set; }
}