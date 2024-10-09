namespace MyServe.Backend.Common.Options;

public struct StoredObjectInfo
{
    public string Bucket { get; }
    public string Key { get; }
    public string AccessUrl { get; }

    public StoredObjectInfo(string bucket, string key, string accessUrl)
    {
        Bucket = bucket;
        Key = key;
        AccessUrl = accessUrl;
    }

    public static readonly StoredObjectInfo Empty = new StoredObjectInfo(string.Empty, string.Empty, string.Empty);
}