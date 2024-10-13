using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.App.Application.Extensions;
using MyServe.Backend.App.Application.Messages.Files;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Abstracts;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.Common.Models;
using Serilog;

namespace MyServe.Backend.Worker.MessageConsumer.Consumer.Files;

public class DeleteObjectConsumer(ILogger logger,
    [FromKeyedServices(ServiceKeyConstants.Storage.FileStorage)] IStorageClient fileStorageClient,
    [FromKeyedServices(ServiceKeyConstants.Storage.FileStorage)] BucketCustomConfiguration fileBucketConfiguration,
    [FromKeyedServices(ServiceKeyConstants.Storage.FileStorage)] IStorageClient profileStorageClient,
    [FromKeyedServices(ServiceKeyConstants.Storage.FileStorage)] BucketCustomConfiguration profileBucketConfiguration,
    IReadWriteUnitOfWork readWriteUnitOfWork,
    IFileService fileService
    ) : IConsumer<ObjectDeleteionEvent>
{
    public async Task Consume(ConsumeContext<ObjectDeleteionEvent> context)
    {
        logger.Information("Received file deletion request for {Identification}", string.Join(", ", context.Message.Files.Select(
            x => string.IsNullOrWhiteSpace(x.Value) ? $"Directory: {x.Key}" : "Object: {x.Value}")));

        List<Guid> failedIds = [];
        
        var (bucketConfiguration, storageClient) = GetServicesFor(context.Message);
        var bucketConfigurationsById = context.Message.Files
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => (x.Key, bucketConfiguration.FromUrl(new Uri(x.Value!))));

        List<Uri> accessUris = new List<Uri>();
        var configurationsById = bucketConfigurationsById.ToList();
        foreach (var (key, storedObjectInfo) in configurationsById)
        {
            if (storedObjectInfo.HasValue)
            {
                accessUris.Add(storedObjectInfo.Value.AccessUrl);
            }
            else
            {
                failedIds.Add(key);
                logger.Warning("Failed to determine the key for {FileId} on {Url}", context.Message.Files[key] ?? "N/A", key );
            }
        }

        var failedObjects = await storageClient.DeleteMultipleAsync(accessUris);

        var failedStorageObjects = failedObjects.Select(x => bucketConfiguration.FromUrl(x)).ToList();
        failedIds.AddRange(configurationsById.Where(x => x.Item2 != null && failedStorageObjects.Contains(x.Item2)).Select(x => x.Key));

        await using var uow = await readWriteUnitOfWork.StartTransactionAsync();
        try
        {
            if (context.Message.FileSource == FileSource.Files)
            {
                var deletedKeys = context.Message.Files.Select(x => x.Key).Except(failedIds).ToList();
                await fileService.HardDeleteAsync(deletedKeys);
            }
        }
        catch (Exception e)
        {
            await uow.RollbackAsync();
            logger.Fatal("Failed to delete the files from storage ");
        }
    }

    private (BucketCustomConfiguration bucketConfiguration, IStorageClient storageClient) GetServicesFor(ObjectDeleteionEvent request)
    {
        return request.FileSource switch
        {
            FileSource.Files => (fileBucketConfiguration, fileStorageClient),
            _ => (profileBucketConfiguration, profileStorageClient)
        };
    }
}