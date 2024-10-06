using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.Common.Constants.StorageConstants;
using MyServe.Backend.Common.Options;

namespace MyServe.Backend.App.Application.Features.Files.Signed;

public class CreateSignedUrlCommandHandler(
    IRequestContext requestContext,
    [FromKeyedServices(ServiceKeyConstants.Storage.FileStorage)] IStorageClient fileStorageClient,
    [FromKeyedServices(ServiceKeyConstants.Storage.ProfileStorage)] IStorageClient profileStorageClient
    ) : IRequestHandler<CreateSignedUrlCommand, CreateSignedUrlResponse>
{
    public async Task<CreateSignedUrlResponse> Handle(CreateSignedUrlCommand request, CancellationToken cancellationToken)
    {
        return request.SourceParsed switch
        {
            PublicSignedUrlRequestType.Files => await ForFile(request, cancellationToken),
            _ => new CreateSignedUrlResponse()
            {
                Success = false,
                Message = "Url could not be created"
            }
        };
    }

    private async Task<CreateSignedUrlResponse> ForFile(CreateSignedUrlCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var requesterUserId = requestContext.Requester.UserId;
            var routePath = StorageRoutes.Files.Frame(requesterUserId.ToString(), request.ParentId?.ToString() ?? string.Empty, request.FileName);

            var generatePreSignedUrlAsync = await fileStorageClient.GeneratePreSignedUrlAsync(new SignedStorageAccessOptions()
            {
                Action = SignedStorageAccessOptions.Upload,
                TimeToLive = request.Duration
            }, routePath);

            return new CreateSignedUrlResponse()
            {
                Success = true,
                AccessUrl = generatePreSignedUrlAsync
            };
        }
        catch (Exception e)
        {
            return new CreateSignedUrlResponse()
            {
                Success = false,
                Message = $"Failed to generate pre-signed url: {e.Message}"
            };
        }
    }
}