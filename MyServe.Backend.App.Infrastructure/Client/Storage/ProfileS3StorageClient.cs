using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.Common.Models;
using Serilog;

namespace MyServe.Backend.App.Infrastructure.Client.Storage;

public class ProfileS3StorageClient([FromKeyedServices(ServiceKeyConstants.Storage.ProfileStorage)]IAmazonS3 s3Client, ILogger logger, [FromKeyedServices(ServiceKeyConstants.Storage.ProfileStorage)] BucketCustomConfiguration bucketCustomConfiguration) : S3StorageClient(s3Client, logger, bucketCustomConfiguration), IStorageClient
{
    
}