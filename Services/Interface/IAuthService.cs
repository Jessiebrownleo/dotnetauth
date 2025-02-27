using DotnetAuthentication.Models.DTOs;

namespace DotnetAuthentication.Services.Interface;

public interface IAuthService
{
    Task<UserResponse> LoginAsync(LoginRequest request);
    Task RegisterAsync(RegisterRequest request);
    Task SendEmailVerificationAsync(string email);
    Task VerifyEmailAsync(string email, string otp);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(string email, string token, string newPassword);
    Task<UserResponse> GoogleLoginAsync(string idToken);
}