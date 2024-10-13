using MyServe.Backend.Common.Models;
using MyServe.Backend.Common.Options;

namespace MyServe.Backend.App.Application.Client;

public interface IStorageClient
{
    /**
     * Upload a file to the dedicated storage space
     */
    Task<Uri?> UploadAsync(FileContent content, bool publicRead = false, params string[] filePath);
    
    /**
     * Deletes a file from the storage space
     */
    Task DeleteAsync(Uri uri);
    
    /**
     * 
     */
    Task<IEnumerable<Uri>> DeleteMultipleAsync(IEnumerable<Uri> uris);
    
    /**
     * Generate a pre-signed url
     */
    Task<Uri> GeneratePreSignedUrlAsync(SignedStorageAccessOptions accessOptions, params string[] filePath);
}