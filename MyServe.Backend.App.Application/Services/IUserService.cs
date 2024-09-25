using MyServe.Backend.App.Domain.Models.User;

namespace MyServe.Backend.App.Application.Services;

public interface IUserService
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(Guid userId);
}