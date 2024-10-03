using MyServe.Backend.Common.Models;

namespace MyServe.Backend.App.Application.Client;

public interface IStorageClient
{
    Task<Uri?> UploadAsync(FileContent content, bool publicRead = false, params string[] filePath);
    
    Task DeleteAsync(Uri uri);
    
    Task<string> GeneratePreSignedUrlAsync();
}