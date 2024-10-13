using MyServe.Backend.App.Application.Client;
using MyServe.Backend.Common.Exceptions.Storage;
using MyServe.Backend.Common.Models;
using MyServe.Backend.Common.Options;
using Serilog;
using Supabase.Storage;
using FileOptions = Supabase.Storage.FileOptions;

namespace MyServe.Backend.App.Infrastructure.Client.Storage;

public class SupabaseStorageClient(Supabase.Client supabaseClient, ILogger logger) : IStorageClient
{
    public async Task<Uri> UploadAsync(FileContent fileContent,bool publicRead = false, params string[] filePath)
    {
        if(filePath.Length < 2)
            throw new ArgumentException("File path should contain at least 2 elements. Bucket and file name", nameof(filePath));

        var bucketName = filePath[0];
        var bucket = await CreateOrGetBucketAsync(bucketName);
        var fileRoute = string.Join('/', filePath.Skip(1));
        
        using MemoryStream memoryStream = new();
        await fileContent.FileStream.CopyToAsync(memoryStream);
        
        var fileUrl = await supabaseClient.Storage
            .From(bucketName)
            .Upload(memoryStream.ToArray(),
                fileRoute,
                options: new FileOptions()
                {
                Upsert = true
                },
                onProgress: ((_, f) => logger.Information($"Uploading file {bucketName}/{fileRoute}: {f}%") ) );
        
        return new Uri(fileUrl);
    }

    public Task DeleteAsync(Uri uri)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Uri>> DeleteMultipleAsync(IEnumerable<Uri> uris)
    {
        throw new NotImplementedException();
    }

    public Task<Uri> GeneratePreSignedUrlAsync(SignedStorageAccessOptions accessOptions, params string[] filePath)
    {
        throw new NotImplementedException();
    }

    private async Task<Bucket> CreateOrGetBucketAsync(string bucketName)
    {
        var bucket = await supabaseClient.Storage.GetBucket(bucketName);
        if (bucket != null)
            return bucket;

        logger.Information("Bucket with name {name} is not present. Trying to create one!", bucketName);
        
        var response = await supabaseClient.Storage.CreateBucket(bucketName, new BucketUpsertOptions()
        {
            Public = false
        });

        bucket = await supabaseClient.Storage.GetBucket(bucketName);
        if(bucket is null)
            throw new FailedBucketGenerationException(bucketName, "Bucket doesn't exist after successful creation with response "+response);
        
        logger.Information("Created bucket with name {name}, Creation response: {response}", bucketName, response);
        return bucket;
    }
}