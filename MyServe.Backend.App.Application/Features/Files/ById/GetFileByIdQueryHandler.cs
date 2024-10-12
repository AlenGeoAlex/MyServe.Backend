using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.App.Application.Extensions;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Exceptions.File;
using MyServe.Backend.App.Domain.Models.Files;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.Common.Models;
using MyServe.Backend.Common.Options;

namespace MyServe.Backend.App.Application.Features.Files.ById;

public class GetFileByIdQueryHandler(
    IFileService fileService,
    [FromKeyedServices(ServiceKeyConstants.Storage.FileStorage)] IStorageClient fileStorageClient,
    [FromKeyedServices(ServiceKeyConstants.Storage.FileStorage)] BucketCustomConfiguration fileStorageConfig,
    IRequestContext requestContext
    ) : IRequestHandler<GetFileByIdQuery, GetFileByIdResponse?>
{
    public async Task<GetFileByIdResponse?> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
    {
        var file = await fileService.GetFileAsync(request.Id);
        if(file is null)
            return null;
        
        if (file.Type.Equals(FileType.Dir.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidDirectoryAccessException(file.Name);
        
        if (file.Owner != requestContext.Requester.UserId)
            throw new InvalidFileAccessException(file.Name);
        
        if(file.TargetUrl is null)
            throw new InvalidFileException("Failed to locate the target file.");
        
        var targetUri = new Uri(file.TargetUrl);
        var storedObjectInfo = fileStorageConfig.FromUrl(targetUri);
        
        if(storedObjectInfo is null)
            throw new InvalidFileAccessException("Failed to match the the target file with the provided storage bucket!");

        var objectInfo = storedObjectInfo.Value;
        var oneHour = TimeSpan.FromHours(1);
        var accessOptions = new SignedStorageAccessOptions()
        {
            Action = SignedStorageAccessOptions.Download,
            TimeToLive = oneHour
        };
        var signedUrl = await fileStorageClient.GeneratePreSignedUrlAsync(accessOptions, objectInfo.Key.Split("/"));
        file.TargetUrl = signedUrl.ToString();
        return new GetFileByIdResponse()
        {
            File = file,
            Expiry = DateTimeOffset.UtcNow.Add(oneHour),
        };
    }
}