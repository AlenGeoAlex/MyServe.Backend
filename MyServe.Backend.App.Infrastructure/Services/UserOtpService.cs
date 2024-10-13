using MassTransit;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.App.Application.Dto.Otp;
using MyServe.Backend.App.Application.Messages.User;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Models.User;
using MyServe.Backend.App.Domain.Repositories;
using MyServe.Backend.Common;

namespace MyServe.Backend.App.Infrastructure.Services;

public class UserOtpService(IUserOtpRepository userOtpRepository, IRandomStringGeneratorClient generatorClient, IUserService userService, IPublishEndpoint publishEndpoint) : IUserOtpService
{
    public async Task<OtpDto> CreateUserOtpAsync(string emailAddress, string requestOrigin, string? device = null, TimeSpan? expiry = null)
    {
        var (activeOtpCount, isAccountLocked, userId) = await userOtpRepository.GetOtpCreationCriteria(emailAddress);
        if (userId == Guid.Empty)
            return new OtpDto()
            {
                UserId = Guid.Empty
            };
        
        if (isAccountLocked)
            return new OtpDto()
            {
                UserId = Guid.Empty,
                Message = "Account is locked!"
            };

        if (activeOtpCount > 5)
            return new OtpDto()
            {
                UserId = Guid.Empty,
                Message = "Wait for some time to create new OTP's"
            };

        var attempt = 0;
        do
        {
            var generatedOtp = generatorClient.Generate();
            try
            {
                await userOtpRepository.AddAsync(new UserOtp()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Expiry = DateTimeOffset.UtcNow.AddMinutes(AppConfigurations.Auth.OtpValidityDurationInMinutes),
                    Device = device ?? "WebApp",
                    Otp = generatedOtp
                });

                await SendEmailCommand(new RequestEmailValidationCommand()
                {
                    Code = generatedOtp,
                    UserId = userId,
                    Device = device ?? "WebApp",
                    Email = emailAddress,
                    RequestOrigin = requestOrigin
                });

                return new OtpDto()
                {
                    UserId = userId,
                    EmailAddress = emailAddress,
                    Otp = generatedOtp
                };
            }
            catch (Exception e)
            {
                attempt++;   
            }
        } while (attempt <= 3);

        return new OtpDto()
        {
            UserId = Guid.Empty,
            Message = "Otp generation failed!"
        };
    }

    public async Task<User?> ValidateUserOtpAsync(string emailAddress, string code, string? device = null)
    {
        var validateOtp = await userOtpRepository.ValidateOtp(code, emailAddress, device ?? "WebApp");
        if(!validateOtp)
            return null;

        return await userService.GetUserByEmailAsync(emailAddress);
    }

    private async Task SendEmailCommand(RequestEmailValidationCommand command)
    {
        await publishEndpoint.Publish(command);
    }
}