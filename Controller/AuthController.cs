using DotnetAuthentication.Models.DTOs;
using DotnetAuthentication.Services.Interface;
using Microsoft.AspNetCore.Mvc;


namespace DotnetAuthentication.Controller;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        await authService.RegisterAsync(request);
        return Ok(new { Message = "User registered. Please check your email for a 6-digit OTP to verify." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string otp)
    {
        await authService.VerifyEmailAsync(email, otp);
        return Ok(new { Message = "Email verified successfully. You can now log in." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await authService.ForgotPasswordAsync(request.Email);
        return Ok(new { Message = "Password reset link sent to your email." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
        return Ok(new { Message = "Password reset successfully." });
    }
    
    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
            var response = await authService.GoogleLoginAsync(request.IdToken);
            return Ok(response);
    }
}