using MyServe.Backend.Common.Models;
using MyServe.Backend.Common.Options;

namespace MyServe.Backend.App.Application.Client;

public interface IStorageClient
{
    Task<Uri?> UploadAsync(FileContent content, bool publicRead = false, params string[] filePath);
    
    Task DeleteAsync(Uri uri);

    Task<Uri> GeneratePreSignedUrlAsync(SignedStorageAccessOptions accessOptions, params string[] filePath);
}