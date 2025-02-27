using System.ComponentModel.DataAnnotations;

namespace DotnetAuthentication.Models.DTOs;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [Display(Description = "The email address associated with the account.")]
    public string Email { get; set; }
}