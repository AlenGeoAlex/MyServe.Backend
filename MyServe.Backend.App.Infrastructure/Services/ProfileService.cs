using MyServe.Backend.App.Application.Dto.Profile;
using MyServe.Backend.App.Application.Features.Profile.Create;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Models.Profile;
using MyServe.Backend.App.Domain.Repositories;
using MyServe.Backend.App.Infrastructure.Mapper.Profile;
using MyServe.Backend.App.Infrastructure.Repositories;

namespace MyServe.Backend.App.Infrastructure.Services;

public class ProfileService(
    IProfileRepository profileRepository
    ) : IProfileService
{
    private static readonly ProfileMapper ProfileMapper = new ProfileMapper();
    
    public async Task<ProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await profileRepository.GetByIdAsync(userId);

        if (profile is null)
            return null;
        
        var profileDto = ProfileMapper.ToProfileDto(profile);
        return profileDto;
    }

    public async Task<ProfileDto> CreateNewProfile(CreateProfileCommand command, CancellationToken cancellationToken = default)
    {
        var profile = new Profile(){
            Id = command.ProfileId,
            FirstName = command.FirstName,
            LastName = command.LastName,
            CreatedAt = DateTimeOffset.UtcNow,
            Settings = new ProfileSettings()
            {
                NotificationEnabled = command.Settings.NotificationEnabled
            },
            EmailAddress = command.Email
        };

        var pro = await profileRepository.AddAsync(profile);

        return ProfileMapper.ToProfileDto(profile);;
    }
}