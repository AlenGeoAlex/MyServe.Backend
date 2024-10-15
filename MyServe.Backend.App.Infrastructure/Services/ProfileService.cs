using System.Net.Mime;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.App.Application.Dto.Me;
using MyServe.Backend.App.Application.Dto.Profile;
using MyServe.Backend.App.Application.Features.Profile.Create;
using MyServe.Backend.App.Application.Features.Profile.Search;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Models.Profile;
using MyServe.Backend.App.Domain.Repositories;
using MyServe.Backend.App.Infrastructure.Mapper.Profile;
using MyServe.Backend.App.Infrastructure.Repositories;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.Common.Constants.StorageConstants;
using MyServe.Backend.Common.Models;
using File = MyServe.Backend.App.Domain.Models.Files.File;

namespace MyServe.Backend.App.Infrastructure.Services;

public class ProfileService(
    IProfileRepository profileRepository,
    [FromKeyedServices(ServiceKeyConstants.Storage.ProfileStorage)] IStorageClient storageClient
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

    public async Task<ProfileDto> CreateNewProfileAsync(CreateProfileCommand command, CancellationToken cancellationToken = default)
    {
        var profile = new Profile(){
            Id = command.ProfileId,
            FirstName = command.FirstName,
            LastName = command.LastName,
            CreatedAt = DateTimeOffset.UtcNow,
            ProfileSettings = new ProfileSettings()
            {
                NotificationEnabled = command.Settings.NotificationEnabled
            },
            EmailAddress = command.Email,
            EncryptionKey = command.EncryptionKey
        };

        
        if (command.ProfileImageStream is not null)
        {
            var fileContent = new FileContent()
            {
                FileStream = command.ProfileImageStream,
                ContentType = string.IsNullOrWhiteSpace(command.ContentType) ? new ContentType() : new ContentType(command.ContentType),
            };
            var profileUrl = await storageClient.UploadAsync(fileContent,true ,StorageRoutes.ProfileImage.Frame(command.ProfileId.ToString()));
            profile.ProfileImageUrl = profileUrl?.ToString();
        }

        
        var pro = await profileRepository.AddAsync(profile);

        return ProfileMapper.ToProfileDto(profile);;
    }

    public async Task<List<SearchDto>> SearchAsync(MeSearchQuery searchQuery, CancellationToken cancellationToken = default)
    {
        var entities = await profileRepository.SearchAcrossProfileAsync(searchQuery.Search, searchQuery.UserId);
        List<SearchDto> responses = [];
        foreach (var entity in entities)
        {
            if (entity is File file)
            {
                responses.Add(new SearchDto()
                {
                    Service = ServiceType.File.ToString(),
                    Name = file.Name,
                    Id = file.Id,
                    Description = "",
                    Metadata =
                    {
                        {"ParentId", file.ParentId?.ToString() },
                        {"Type", file.Type.ToString()},
                        {"MimeType", file.MimeType},
                        {"TargetSize", file.TargetSize.ToString()}
                    }
                });       
            }
        }
        return responses;
    }
}