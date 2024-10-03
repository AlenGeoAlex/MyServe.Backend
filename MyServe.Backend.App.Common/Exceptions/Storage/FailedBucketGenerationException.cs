namespace MyServe.Backend.Common.Exceptions.Storage;

public class FailedBucketGenerationException(string bucketName, string? reason = null) : Exception($"Failed to create bucket {bucketName}. Reason if present: {reason}") { }