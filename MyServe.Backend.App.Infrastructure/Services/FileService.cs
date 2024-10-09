using MyServe.Backend.App.Application.Dto.Files;
using MyServe.Backend.App.Application.Features.Files.Create;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Exceptions.File;
using MyServe.Backend.App.Domain.Extensions;
using MyServe.Backend.App.Domain.Models.Files;
using MyServe.Backend.App.Domain.Repositories;
using MyServe.Backend.App.Infrastructure.Mapper.File;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Options;
using File = MyServe.Backend.App.Domain.Models.Files.File;

namespace MyServe.Backend.App.Infrastructure.Services;

public class FileService(IFileRepository fileRepository, IRequestContext requestContext) : IFileService
{
    
    private static readonly FileMapper FileMapper = new FileMapper();

    public async Task<FileDto?> GetFile(Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId);
        if (file == null)
            return null;

        return FileMapper.ToFileDto(file);
    }

    public async Task<FileDto> Create(CreateFileCommand command)
    {
        var file = new File()
        {
            Id = Guid.NewGuid(),
            Created = command.Owner,
            Owner = command.Owner,
            ParentId = command.ParentId,
            CreatedAt = DateTimeOffset.UtcNow,
            MimeType = command.MimeType,
            Name = command.Name,
            Type = command.Type.GetFileTypeFromString()!.Value,
            TargetSize = command.TargetSize,
            TargetUrl = command.TargetUrl
        };

        await fileRepository.AddAsync(file);
        var fileDto = FileMapper.ToFileDto(file);
        return fileDto;
    }

    public async Task<(List<FileDto> files, List<FileDto> parents)> ListMyFiles(Guid userId, Guid? parentId, ListOptions listOptions)
    {
        var (files, parents) = await fileRepository.ListFilesWithParent(userId, parentId, listOptions);
        return (files.Select(x => FileMapper.ToFileDto(x)).ToList(), parents.Select(x =>FileMapper.ToFileDto(x)).ToList());
    }
}