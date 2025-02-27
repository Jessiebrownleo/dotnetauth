using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DotnetAuthentication.Models.DTOs;
using DotnetAuthentication.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAuthentication.Controller;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value
                    ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value;


        if (string.IsNullOrEmpty(email))
        {
            return Unauthorized("Invalid token: email claim is missing.");
        }

        Console.WriteLine($"Extracted Email: {email}"); // Log to console

        var user = await userService.GetUserByEmailAsync(email);
        if (user == null)
        {
            return NotFound($"User not found in database. Extracted Email: {email}");
        }

        return Ok(new UserResponse { Email = user.Email, Username = user.Username });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await userService.GetAllUsersAsync();
        return Ok(users.Select(u => new UserResponse { Email = u.Email, Username = u.Username }));
    }
}