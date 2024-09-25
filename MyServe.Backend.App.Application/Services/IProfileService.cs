using MyServe.Backend.App.Application.Dto.Profile;
using MyServe.Backend.App.Application.Features.Profile.Create;

namespace MyServe.Backend.App.Application.Services;

public interface IProfileService
{
    Task<ProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task<ProfileDto> CreateNewProfile(CreateProfileCommand command, CancellationToken cancellationToken = default);
}