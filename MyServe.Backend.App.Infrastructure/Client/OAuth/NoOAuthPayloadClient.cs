using MyServe.Backend.App.Application.Client;
using MyServe.Backend.App.Application.Dto.User;
using Serilog;

namespace MyServe.Backend.App.Infrastructure.Client.OAuth;

public class NoOAuthPayloadClient(ILogger logger) : IOAuthPayloadClient
{
    public Task<UserIdentificationDto?> ValidateAccessPayloadAsync(string payload)
    {
        logger.Information("Rejecting validating since the OAuth is not configured");
        return Task.FromResult<UserIdentificationDto?>(null);
    }
}