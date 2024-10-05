using MyServe.Backend.App.Application.Dto.Files;
using MyServe.Backend.App.Application.Features.Files.Create;
using MyServe.Backend.Common.Options;

namespace MyServe.Backend.App.Application.Services;

public interface IFileService
{

    public Task<FileDto> Create(CreateFileCommand command);

    public Task<List<FileDto>> ListMyFiles(Guid userId, Guid? parentId, ListOptions listOptions);

}