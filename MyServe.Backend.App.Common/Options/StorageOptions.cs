using MyServe.Backend.Common.Constants;

namespace MyServe.Backend.Common.Options;

public record StorageOptions(StorageType Type, string VaultPrefix, string? CustomDomain = null);

public class StorageConfiguration
{
    public StorageOptions Profile { get; set; }
    public StorageOptions Files { get; set; }
}