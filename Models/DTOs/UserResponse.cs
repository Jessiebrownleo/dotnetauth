namespace DotnetAuthentication.Models.DTOs;

public class UserResponse
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
}