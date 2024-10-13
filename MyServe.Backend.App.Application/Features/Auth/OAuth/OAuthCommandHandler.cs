using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.App.Application.Dto.User;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Abstracts;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.Common.Impl;
using Serilog;

namespace MyServe.Backend.App.Application.Features.Auth.OAuth;

public class OAuthCommandHandler(
    [FromKeyedServices(ServiceKeyConstants.OAuthValidator.Google)] IOAuthPayloadClient googleValidator,
    IUserService userService,
    IRefreshTokenService refreshTokenService,
    IAccessTokenClient accessTokenClient,
    IReadWriteUnitOfWork readWriteUnitOfWork,
    ILogger logger
    ) : IRequestHandler<OAuthCommand, OAuthResponse>
{
    public async Task<OAuthResponse> Handle(OAuthCommand request, CancellationToken cancellationToken)
    {
        
        var userIdentificationDto = await ValidateForSource(request);
        if (userIdentificationDto is null)
            return new OAuthResponse()
            {
                Success = false,
                Message = "Failed to validate the authenticity of the provided access token"
            };
        await using var uow = await readWriteUnitOfWork.StartTransactionAsync();
        try
        {
            var user = await userService.GetUserByEmailAsync(userIdentificationDto.Email);
            if (user is null)
            {
                return new OAuthResponse()
                {
                    Success = false,
                    Message =
                        "You don't have an account in the system. This is a private system and won't be provided access to outside users!"
                };
            }
            
            var accessTokenTask = accessTokenClient.CreateTokenAsync(new TokenGenerationOption()
            {
                UserId = user.Id,
                Device = request.Device,
                Email = user.EmailAddress
            });

            var refreshTokenTask = refreshTokenService.CreateRefreshTokenAsync(user.Id);

            await Task.WhenAll(accessTokenTask, refreshTokenTask);
            
            return new OAuthResponse()
            {
                Success = true,
                AccessToken = accessTokenTask.Result,
                RefreshToken = refreshTokenTask.Result.ToString()
            };
        }
        catch (Exception e)
        {
            logger.Error(e, "An unknown error occured while generating the user access for {EmailAddress}", userIdentificationDto.Email);
            return new OAuthResponse()
            {
                Success = false,
                Message = "An unknown error occured while processing your request"
            };
        }
    }

    private async Task<UserIdentificationDto?> ValidateForSource(OAuthCommand command)
    {
        if (OAuthTypes.Google.ToString() == command.OAuthType)
        {
            return await googleValidator.ValidateAccessPayloadAsync(command.Identity);
        }

        return null;
    } 
}