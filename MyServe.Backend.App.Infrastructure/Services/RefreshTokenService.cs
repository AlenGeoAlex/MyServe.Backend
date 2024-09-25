using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Models.User;
using MyServe.Backend.App.Domain.Repositories;

namespace MyServe.Backend.App.Infrastructure.Services;

public class RefreshTokenService(IRefreshTokenRepository repository) : IRefreshTokenService
{
    public async Task<Guid> CreateRefreshTokenAsync(Guid userId)
    {
        var refreshTokenId = Guid.NewGuid();
        var refreshToken = new RefreshToken()
        {
            Id = refreshTokenId,
            UserId = userId,
            Expiry = DateTimeOffset.Now.AddDays(7),
            CreatedAt = DateTimeOffset.Now
        };

        await repository.AddAsync(refreshToken);
        return refreshTokenId;
    }

    public async Task<Guid?> ValidateAndGetUserIdByTokenAsync(Guid refreshToken)
    {
        var token = await repository.GetByIdAsync(refreshToken);
        if (token == null)
            return null;

        if (token.Expiry < DateTimeOffset.UtcNow)
            return null;
        
        await repository.DeleteByIdAsync(refreshToken);
        return token.UserId;
    }
}