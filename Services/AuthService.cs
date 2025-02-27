using System.Security.Cryptography;
using DotnetAuthentication.Data;
using DotnetAuthentication.Helper;
using DotnetAuthentication.Models.DTOs;
using DotnetAuthentication.Models.Entities;
using DotnetAuthentication.Services.Interface;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;

namespace DotnetAuthentication.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly EmailService _emailService;

    public AuthService(AppDbContext context, IConfiguration config, EmailService emailService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    public async Task<UserResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw Exceptions.Unauthorized("Invalid credentials");

        var token = JwtHelper.GenerateJwtToken(user, _config);
        user.RefreshToken = JwtHelper.GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(_config["Jwt:RefreshTokenLifetimeDays"]));
        await _context.SaveChangesAsync();

        return new UserResponse { Token = token, RefreshToken = user.RefreshToken, Email = user.Email, Username = user.Username };
    }

    public async Task RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            throw Exceptions.BadRequest("Email already in use");

        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole == null)
            throw Exceptions.NotFound("Default 'User' role not found");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            RoleId = userRole.Id,
            EmailVerificationToken = GenerateOtp()
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var frontendUrl = _config["FrontendUrl"] ?? "https://dotnetauthentication-ui.soben.me";
        var body = GenerateEmailTemplate(
            "Email Verification",
            $"Hello {user.Username},",
            "Thank you for registering! Please use the OTP below to verify your email:",
            $"<p style='text-align: center;'><strong style='font-size: 24px; color: #007bff;'>{user.EmailVerificationToken}</strong></p>",
            "If you didn’t register, please ignore this email."
        );
        await _emailService.SendEmailAsync(user.Email, "Verify Your Email", body);
    }

    public async Task SendEmailVerificationAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw Exceptions.NotFound("User not found");
        if (user.IsEmailVerified) throw Exceptions.Forbidden("Email already verified");

        user.EmailVerificationToken = GenerateOtp();
        await _context.SaveChangesAsync();

        var frontendUrl = _config["FrontendUrl"] ?? "https://dotnetauthentication-ui.soben.me";
        var body = GenerateEmailTemplate(
            "Email Verification",
            $"Hello {user.Username},",
            "Please use the OTP below to verify your email:",
            $"<p style='text-align: center;'><strong style='font-size: 24px; color: #007bff;'>{user.EmailVerificationToken}</strong></p>",
            "If you didn’t request this, please ignore this email."
        );
        await _emailService.SendEmailAsync(email, "Verify Your Email", body);
    }

    public async Task VerifyEmailAsync(string email, string otp)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw Exceptions.NotFound("User not found");
        if (user.IsEmailVerified) throw Exceptions.Forbidden("Email already verified");
        if (user.EmailVerificationToken != otp) throw Exceptions.BadRequest("Invalid OTP");

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        await _context.SaveChangesAsync();
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw Exceptions.NotFound("User not found");
        if (!user.IsEmailVerified) throw Exceptions.Forbidden("Email not verified");

        user.ResetPasswordToken = GenerateToken();
        user.ResetPasswordTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        var frontendUrl = _config["FrontendUrl"] ?? "https://dotnetauthentication-ui.soben.me";
        var resetLink = $"{frontendUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(user.ResetPasswordToken)}";
        var body = GenerateEmailTemplate(
            "Password Reset Request",
            $"Hello {user.Username},",
            "We received a request to reset your password. Click the button below to proceed:",
            $"<p style='text-align: center;'><a href='{resetLink}' class='button'>Reset Password</a></p>",
            "This link will expire in <strong>1 hour</strong>. If you didn’t request this, please ignore this email."
        );
        await _emailService.SendEmailAsync(email, "Reset Your Password", body);
    }

    public async Task ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) throw Exceptions.NotFound("User not found");
        if (!user.IsEmailVerified) throw Exceptions.Forbidden("Email not verified");
        if (user.ResetPasswordToken != token || user.ResetPasswordTokenExpiry < DateTime.UtcNow)
            throw Exceptions.BadRequest("Invalid or expired token");

        user.PasswordHash = PasswordHasher.HashPassword(newPassword);
        user.ResetPasswordToken = null;
        user.ResetPasswordTokenExpiry = null;
        await _context.SaveChangesAsync();
    }
    
    public async Task<UserResponse> GoogleLoginAsync(string idToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _config["GoogleAuth:ClientId"] }
        };
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

        var user = await _context.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == payload.Email);
        if (user == null)
        {
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null) throw Exceptions.NotFound("Default 'User' role not found");

            user = new User
            {
                Username = payload.Email.Split('@')[0],
                Email = payload.Email,
                PasswordHash = "",
                RoleId = userRole.Id,
                IsEmailVerified = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var token = JwtHelper.GenerateJwtToken(user, _config);
        user.RefreshToken = JwtHelper.GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(double.Parse(_config["Jwt:RefreshTokenLifetimeDays"]));
        await _context.SaveChangesAsync();

        return new UserResponse { Token = token, RefreshToken = user.RefreshToken, Email = user.Email, Username = user.Username };
    }

    private string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString("D6");
    }

    private string GenerateToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private string GenerateEmailTemplate(string title, string greeting, string intro, string actionContent, string disclaimer)
    {
        return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>{title}</title>
                <style>
                    body {{
                        font-family: 'Arial', sans-serif;
                        background-color: #f4f4f4;
                        margin: 0;
                        padding: 0;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 40px auto;
                        background-color: #ffffff;
                        border-radius: 8px;
                        box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                        overflow: hidden;
                    }}
                    .header {{
                        background-color: #007bff;
                        color: #ffffff;
                        padding: 20px;
                        text-align: center;
                    }}
                    .content {{
                        padding: 30px;
                        color: #333333;
                    }}
                    .button {{
                        display: inline-block;
                        padding: 12px 24px;
                        background-color: #007bff;
                        color: #ffffff;
                        text-decoration: none;
                        border-radius: 5px;
                        font-weight: bold;
                        text-align: center;
                    }}
                    .button:hover {{
                        background-color: #0056b3;
                    }}
                    .footer {{
                        text-align: center;
                        padding: 20px;
                        font-size: 12px;
                        color: #777777;
                        background-color: #f8f9fa;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>{title}</h1>
                    </div>
                    <div class='content'>
                        <p>{greeting}</p>
                        <p>{intro}</p>
                        {actionContent}
                        <p>{disclaimer}</p>
                    </div>
                    <div class='footer'>
                        <p>© {DateTime.UtcNow.Year} DotnetAuthentication. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";
    }
}