using DotnetAuthentication.Data;
using DotnetAuthentication.Models.Entities;
using DotnetAuthentication.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace DotnetAuthentication.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }


    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }


    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }
}