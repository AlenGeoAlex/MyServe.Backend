using MediatR;
using MyServe.Backend.App.Application.Dto.Files;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Constants;

namespace MyServe.Backend.App.Application.Features.Files.Patch;

public class FilePatchHandler(IFileService fileService, IRequestContext requestContext) : IRequestHandler<FilePatchCommand, FileDto>
{
    public async Task<FileDto> Handle(FilePatchCommand request, CancellationToken cancellationToken)
    {
        var fileDto = await fileService.PatchAsync(request);
        requestContext.CacheControl.AddKeyToExpire(CacheConstants.FileListCacheKey, fileDto.Owner.ToString(), fileDto.ParentId.HasValue ? fileDto.ParentId.Value.ToString() : "undefined");
        requestContext.CacheControl.AddExactKeyToExpire(CacheConstants.FileIdCacheKey, fileDto.Id.ToString());
        return fileDto;
    }
}