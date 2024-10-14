using MediatR;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Models.Files;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Constants;

namespace MyServe.Backend.App.Application.Features.Files.Delete;

public class DeleteFileCommandHandler(IFileService fileService, IRequestContext context) : IRequestHandler<DeleteFileCommand, DeleteFileResponse>
{
    public async Task<DeleteFileResponse> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var deletedFiles = await fileService.SoftDeleteAsync(request);
        foreach (var deletedFile in deletedFiles)
        {
            if (FileType.Dir.ToString().Equals(deletedFile.Type, StringComparison.OrdinalIgnoreCase))
            {
                context.CacheControl.AddKeyToExpire(CacheConstants.FileListCacheKey, context.Requester.UserId.ToString());
            }
            else
            {
                context.CacheControl.AddExactKeyToExpire(CacheConstants.FileIdCacheKey, deletedFile.Id.ToString());
                context.CacheControl.AddKeyToExpire(CacheConstants.FileListCacheKey, context.Requester.UserId.ToString());
            }
        }
        return new DeleteFileResponse();
    }
}