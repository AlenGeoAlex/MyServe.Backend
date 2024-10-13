using MyServe.Backend.Common.Models;
using MyServe.Backend.Common.Options;

namespace MyServe.Backend.App.Application.Extensions;

public static class S3Extensions
{

    
    public static StoredObjectInfo? FromUrl(this BucketCustomConfiguration bucketCustomConfiguration, Uri uri)
    {
        return FromBucketName(bucketCustomConfiguration.Bucket, uri);
    }

    public static StoredObjectInfo? FromBucketName(this string bucketName, Uri uri)
    {
        var bucketInHost = false;
        var uriHost = uri.Host;
        var firstInHost = uriHost.Split(".").First();
        if (firstInHost.Equals(bucketName, StringComparison.OrdinalIgnoreCase))
            bucketInHost = true;

        string key;
        
        if (bucketInHost)
        {
            key = uri.AbsolutePath.TrimStart('/');;
        }
        else
        {
            var keyPath = uri.AbsolutePath.Split("/");
            if (keyPath.Length == 1)
                return null;

            if (!keyPath.First().Equals(bucketName, StringComparison.OrdinalIgnoreCase))
                return null;
            
            key = string.Join("/", keyPath.Skip(1));
        }

        return new StoredObjectInfo(bucketName, key, uri);
    }
    
}