namespace MyServe.Backend.App.Application.Services;

public interface IRefreshTokenService
{
    Task<Guid> CreateRefreshTokenAsync(Guid userId);
    
    Task<Guid?> ValidateAndGetUserIdByTokenAsync(Guid refreshToken); 
}