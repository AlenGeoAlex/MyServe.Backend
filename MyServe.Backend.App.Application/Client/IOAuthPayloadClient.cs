using MyServe.Backend.App.Application.Dto.User;

namespace MyServe.Backend.App.Application.Client;

public interface IOAuthPayloadClient
{
    Task<UserIdentificationDto?> ValidateAccessPayloadAsync(string payload);
}