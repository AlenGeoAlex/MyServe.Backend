using MyServe.Backend.App.Domain.Abstracts;
using MyServe.Backend.App.Domain.Models.User;

namespace MyServe.Backend.App.Domain.Repositories;

public interface IRefreshTokenRepository : IAppRepository<RefreshToken>
{
    Task<bool> ValidateRefreshToken(RefreshToken token);
}