using Google.Apis.Auth;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.App.Application.Dto.User;
using MyServe.Backend.Common.Options.OAuth;
using Serilog;

namespace MyServe.Backend.App.Infrastructure.Client.OAuth;

public class GoogleOAuthPayloadClient(ILogger logger, GoogleOAuthOptions oAuthOptions) : IOAuthPayloadClient
{
    public async Task<UserIdentificationDto?> ValidateAccessPayloadAsync(string payload)
    {
        GoogleJsonWebSignature.Payload? googlePayload;
        try
        {
            googlePayload = await GoogleJsonWebSignature.ValidateAsync(payload);
        }
        catch (Exception e)
        {
            logger.Error(e, "Failed to validate the provided payload. {Payload}", payload);
            return null;
        }

        if (!googlePayload.Issuer.Equals(oAuthOptions.Issuer, StringComparison.OrdinalIgnoreCase))
        {
            logger.Error("The issuer in the payload doesn't match the expected one. The issuer provided is {Issuer} and expected is {Expected}", googlePayload.Issuer, oAuthOptions.Issuer);
            return null;
        }

        if (!googlePayload.AudienceAsList.Any(x => x.Equals(oAuthOptions.ClientId, StringComparison.OrdinalIgnoreCase)))
        {
            logger.Error("The audience in the payload doesn't match the expected one. The audience provided is {Audience} and expected is {Expected}", string.Join(',', googlePayload.AudienceAsList), oAuthOptions.Issuer);
            return null;
        }
        
        DateTime now = DateTime.Now.ToUniversalTime();
        DateTime expiration = DateTimeOffset.FromUnixTimeSeconds(googlePayload.ExpirationTimeSeconds ?? 0).DateTime;
        if (now > expiration)
        {
            logger.Error("The provided token is already expired!");
            return null;
        }

        return new UserIdentificationDto()
        {
            Id = Guid.Empty,
            Email = googlePayload.Email,
            FirstName = googlePayload.GivenName,
            LastName = googlePayload.FamilyName,
        };
    }
}