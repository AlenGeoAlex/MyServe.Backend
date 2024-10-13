namespace MyServe.Backend.Common.Options;

public struct StoredObjectInfo : IEquatable<StoredObjectInfo>
{
    public string Bucket { get; }
    public string Key { get; }
    public Uri AccessUrl { get; }

    public StoredObjectInfo(string bucket, string key, Uri accessUrl)
    {
        Bucket = bucket;
        Key = key;
        AccessUrl = accessUrl;
    }

    public bool Equals(StoredObjectInfo other)
    {
        return Bucket == other.Bucket && Key == other.Key;
    }

    public override bool Equals(object? obj)
    {
        return obj is StoredObjectInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Bucket, Key);
    }

    private sealed class BucketKeyEqualityComparer : IEqualityComparer<StoredObjectInfo>
    {
        public bool Equals(StoredObjectInfo x, StoredObjectInfo y)
        {
            return x.Bucket == y.Bucket && x.Key == y.Key;
        }

        public int GetHashCode(StoredObjectInfo obj)
        {
            return HashCode.Combine(obj.Bucket, obj.Key);
        }
    }

    public static IEqualityComparer<StoredObjectInfo> BucketKeyComparer { get; } = new BucketKeyEqualityComparer();
}