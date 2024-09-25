using MyServe.Backend.App.Application.Dto.Profile;
using Riok.Mapperly.Abstractions;

namespace MyServe.Backend.App.Infrastructure.Mapper.Profile;

[Mapper]
public partial class ProfileMapper
{
    public partial ProfileDto ToProfileDto(Domain.Models.Profile.Profile profile);
}