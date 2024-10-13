using MyServe.Backend.App.Application.Dto.Files;
using MyServe.Backend.App.Application.Features.Files.Create;
using MyServe.Backend.App.Application.Features.Files.Delete;
using MyServe.Backend.App.Application.Features.Files.Patch;
using MyServe.Backend.Common.Options;

namespace MyServe.Backend.App.Application.Services;

public interface IFileService
{
    public Task<FileDto?> GetFileAsync(Guid fileId);
    public Task<FileDto> CreateAsync(CreateFileCommand command);
    public Task<(List<FileDto> files, List<FileDto> parents)> ListMyFilesAsync(Guid userId, Guid? parentId, ListOptions listOptions);
    public Task<FileDto> PatchAsync(FilePatchCommand filePatch);
    public Task<List<FileDto>> SoftDeleteAsync(DeleteFileCommand fileId);
    public Task HardDeleteAsync(List<Guid> fileIds);
}