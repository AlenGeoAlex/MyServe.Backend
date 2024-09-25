using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Models.User;
using MyServe.Backend.App.Domain.Repositories;

namespace MyServe.Backend.App.Infrastructure.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public Task<User?> GetUserByEmailAsync(string email)
    {
        return userRepository.GetByEmail(email);
    }

    public Task<User?> GetUserByIdAsync(Guid userId)
    {
        return userRepository.GetByIdAsync(userId);
    }
}