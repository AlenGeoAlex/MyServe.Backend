using MediatR;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Abstracts;
using MyServe.Backend.Common.Impl;
using Serilog;

namespace MyServe.Backend.App.Application.Features.Auth.Refresh;

public class RefreshTokenCommandHandler(IReadWriteUnitOfWork readWriteUnitOfWork, IUserService userService, ILogger logger, IRefreshTokenService refreshTokenService, IAccessTokenClient tokenClient) : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse?>
{
    public async Task<RefreshTokenResponse?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        await using var uow = await readWriteUnitOfWork.StartTransactionAsync();
        try
        {
            var userId = await refreshTokenService.ValidateAndGetUserIdByTokenAsync(request.TokenId);
            if(userId == null || userId == Guid.Empty)
                return null;

            var user = await userService.GetUserByIdAsync(userId.Value);
            if (user == null || user.IsLocked)
                return null;

            var accessToken = await tokenClient.CreateTokenAsync(new TokenGenerationOption()
            {
                UserId = user.Id,
                Email = user.EmailAddress,
                Device = request.Device
            });
            var tokenResponse = await refreshTokenService.CreateRefreshTokenAsync(userId.Value);
            
            await uow.CommitAsync();
            return new RefreshTokenResponse()
            {
                AccessToken = accessToken,
                RefreshToken = tokenResponse.ToString()
            };
        }
        catch (Exception e)
        {
            logger.Error(e, e.Message);
            await uow.RollbackAsync();
            return null;
        }
    }
}