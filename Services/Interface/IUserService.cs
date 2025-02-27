using DotnetAuthentication.Models.Entities;

namespace DotnetAuthentication.Services.Interface;

public interface IUserService
{
    Task<User> GetUserByEmailAsync(string email);
    Task<List<User>> GetAllUsersAsync();
}