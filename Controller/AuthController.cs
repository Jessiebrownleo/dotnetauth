using DotnetAuthentication.Models.DTOs;
using DotnetAuthentication.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAuthentication.Controller; 
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            await _authService.RegisterAsync(request);
            return Ok(new { Message = "User registered. Please check your email for a 6-digit OTP to verify." });
        }
        catch (Exception ex)
        {
            // Handle known exceptions from AuthService
            if (ex.Message == "Email already in use")
            {
                return BadRequest(new { Message = ex.Message });
            }
            // Log unexpected errors (optional) and return generic error
            Console.WriteLine($"Register error: {ex.Message}");
            return StatusCode(500, new { Message = "An error occurred during registration." });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            if (ex.Message == "Invalid credentials")
            {
                return Unauthorized(new { Message = ex.Message });
            }
            Console.WriteLine($"Login error: {ex.Message}");
            return StatusCode(500, new { Message = "An error occurred during login." });
        }
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string otp)
    {
        try
        {
            await _authService.VerifyEmailAsync(email, otp);
            return Ok(new { Message = "Email verified successfully. You can now log in." });
        }
        catch (Exception ex)
        {
            if (ex.Message == "User not found" || ex.Message == "Email already verified" || ex.Message == "Invalid OTP")
            {
                return BadRequest(new { Message = ex.Message });
            }
            Console.WriteLine($"Verify email error: {ex.Message}");
            return StatusCode(500, new { Message = "An error occurred during email verification." });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            await _authService.ForgotPasswordAsync(request.Email);
            return Ok(new { Message = "Password reset link sent to your email." });
        }
        catch (Exception ex)
        {
            if (ex.Message == "User not found" || ex.Message == "Email not verified")
            {
                return BadRequest(new { Message = ex.Message });
            }
            Console.WriteLine($"Forgot password error: {ex.Message}");
            return StatusCode(500, new { Message = "An error occurred during forgot password request." });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
            return Ok(new { Message = "Password reset successfully." });
        }
        catch (Exception ex)
        {
            if (ex.Message == "User not found" || ex.Message == "Email not verified" || ex.Message == "Invalid or expired token")
            {
                return BadRequest(new { Message = ex.Message });
            }
            Console.WriteLine($"Reset password error: {ex.Message}");
            return StatusCode(500, new { Message = "An error occurred during password reset." });
        }
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
    {
        try
        {
            var response = await _authService.GoogleLoginAsync(request.IdToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("email/password account"))
            {
                return BadRequest(new { Message = ex.Message });
            }
            if (ex.Message == "Default 'User' role not found")
            {
                return BadRequest(new { Message = ex.Message });
            }
            Console.WriteLine($"Google login error: {ex.Message}");
            return StatusCode(500, new { Message = "An error occurred during Google login." });
        }
    }
}