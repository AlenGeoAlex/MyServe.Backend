namespace MyServe.Backend.Common.Constants;

public static class ServiceKeyConstants
{
    public static class Database
    {
        public const string ReadWriteDatabase = "read-write-connection";
        public const string ReadDatabase = "read-only-connection";
    }

    public static class Storage
    {
        public const string ProfileStorage = "profile-storage";
        public const string FileStorage = "file-storage";
    }

    public static class OAuthValidator
    {
        public const string Google = "oauth-validator-google";
    }
}