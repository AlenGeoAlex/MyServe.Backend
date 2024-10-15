using MyServe.Backend.App.Application.Dto.Me;
using MyServe.Backend.App.Application.Dto.Profile;
using MyServe.Backend.App.Application.Features.Profile.Create;
using MyServe.Backend.App.Application.Features.Profile.Search;

namespace MyServe.Backend.App.Application.Services;

public interface IProfileService
{
    Task<ProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ProfileDto> CreateNewProfileAsync(CreateProfileCommand command, CancellationToken cancellationToken = default);
    Task<List<SearchDto>> SearchAsync(MeSearchQuery searchQuery, CancellationToken cancellationToken = default);
}