using System.ComponentModel.DataAnnotations;

namespace DotnetAuthentication.Models.Entities;

public class User
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Username { get; set; } = string.Empty;
    [Required, MaxLength(255)] public string Email { get; set; } = string.Empty;
    [Required] public string PasswordHash { get; set; } = string.Empty;
    public int RoleId { get; set; } // Foreign key to Role table
    public Role Role { get; set; } = null!; // Navigation property
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public string? ResetPasswordToken { get; set; }
    public DateTime? ResetPasswordTokenExpiry { get; set; }
}