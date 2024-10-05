using MyServe.Backend.App.Application.Dto.Files;
using Riok.Mapperly.Abstractions;

namespace MyServe.Backend.App.Infrastructure.Mapper.File;

[Mapper]
public partial class FileMapper
{
    public partial FileDto ToFileDto(Domain.Models.Files.File file);
}