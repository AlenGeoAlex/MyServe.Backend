using MyServe.Backend.App.Domain.Abstracts;
using MyServe.Backend.App.Domain.Models.User;

namespace MyServe.Backend.App.Domain.Repositories;

public interface IUserRepository : IAppRepository<User>
{
    public Task<User?> GetByEmail(string email);

    public Task UpdateLastLogin(Guid userId, DateTimeOffset? lastLogin = null);
}