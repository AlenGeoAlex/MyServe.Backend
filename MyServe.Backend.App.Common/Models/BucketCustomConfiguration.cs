namespace MyServe.Backend.Common.Models;

public record BucketCustomConfiguration(
    string Bucket,
    string RegionBasedUrl,
    string? CustomDomainUrl
)
{
    public string ConstructBucketUrl()
    {
        if(CustomDomainUrl is null)
            return RegionBasedUrl + $"/{Bucket}";
        
        return CustomDomainUrl;
    }
    
    
}