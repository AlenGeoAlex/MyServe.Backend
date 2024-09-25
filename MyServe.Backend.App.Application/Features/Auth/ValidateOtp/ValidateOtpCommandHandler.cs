using MediatR;
using MyServe.Backend.Common.Impl;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Abstracts;
using Serilog;

namespace MyServe.Backend.App.Application.Features.Auth.ValidateOtp;

public class ValidateOtpCommandHandler(ILogger logger, IAccessTokenClient accessTokenClient, IUserOtpService userOtpService, IRefreshTokenService refreshTokenService, IReadWriteUnitOfWork readWriteUnitOfWork) : IRequestHandler<ValidateOtpCommand, ValidateOtpResponse>
{
    public async Task<ValidateOtpResponse> Handle(ValidateOtpCommand request, CancellationToken cancellationToken)
    {
        await using var uow = await readWriteUnitOfWork.StartTransactionAsync();
        try
        {
            var codeValidatedUser = await userOtpService.ValidateUserOtpAsync(request.Email, request.Code.ToUpper(), request.Device);
            if (codeValidatedUser is null)
                return new ValidateOtpResponse()
                {
                    Success = false,
                    Message = "Failed to validate the otp code"
                };

            var accessToken = await accessTokenClient.CreateTokenAsync(new TokenGenerationOption()
            {
                UserId = codeValidatedUser.Id,
                Device = request.Device,
                Email = codeValidatedUser.EmailAddress
            });
            var refreshToken = await refreshTokenService.CreateRefreshTokenAsync(codeValidatedUser.Id);
            await uow.CommitAsync();
            return new ValidateOtpResponse()
            {
                Success = true,
                AccessToken = accessToken,
                Message = $"User {codeValidatedUser.EmailAddress} has been validated.",
                RefreshToken = refreshToken.ToString()
            };
        }
        catch (Exception e)
        {
            logger.Error(e, e.StackTrace ?? e.Message);
            await uow.RollbackAsync();
            return new ValidateOtpResponse()
            {
                Success = false,
                Message = "An error occured while validating your otp.",
            };
        }
    }
}