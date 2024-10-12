using System.Net;
using MediatR;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Abstracts;
using MyServe.Backend.App.Domain.Exceptions;
using MyServe.Backend.App.Domain.Models.Files;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Constants;

namespace MyServe.Backend.App.Application.Features.Files.Create;

public class CreateFileCommandHandler(IFileService fileService, IReadWriteUnitOfWork readWriteUnitOfWork, IRequestContext requestContext) : IRequestHandler<CreateFileCommand, CreateFileResponse>
{
    
    private int exceptionCount = 0;
    
    public async Task<CreateFileResponse> Handle(CreateFileCommand request, CancellationToken cancellationToken)
    {
        await using var uow = await readWriteUnitOfWork.StartTransactionAsync();
        try
        {
            var response = await fileService.CreateAsync(request);
            await uow.CommitAsync();
            requestContext.CacheControl.AddKeyToExpire(CacheConstants.FileListCacheKey, request.Owner.ToString(), request.ParentId.HasValue ? request.ParentId.Value.ToString() : "undefined");
            return new CreateFileResponse()
            {
                File = response
            };
        }
        catch (Exception e)
        {
            await uow.RollbackAsync();
            if (e is not DataWriteFailedException { ConstraintInfo.HttpStatusCode: HttpStatusCode.Conflict }) throw;
            if (exceptionCount >= 3 || request.Type == FileType.Dir.ToString())
                throw;

            exceptionCount++;
            var copyName = GetCopyName(request.Name);
            request.Name = copyName;
            return await Handle(request, cancellationToken);;
        }
    }
    
    private string GetCopyName(string fileName)
    {
        var splitByExtension = fileName.Split(".");
        // Take the extension out if there is one
        var extension = splitByExtension.Length == 1 ? string.Empty : splitByExtension.Last();
        // Remove the extension and make the file name
        var newFileName = splitByExtension.Length == 1 ? splitByExtension.First() : string.Join(".", splitByExtension.Take(splitByExtension.Length - 1));
        string copySuffix = $"-copy-{DateTime.UtcNow.Ticks.ToString()}";
        int maxNewFileNameLength = 100 - copySuffix.Length - extension.Length;
        if (newFileName.Length > maxNewFileNameLength) {
            newFileName = newFileName.Substring(0, maxNewFileNameLength);
        }
        
        return newFileName + copySuffix + (string.IsNullOrEmpty(extension) ? string.Empty : "." + extension);
    }
}