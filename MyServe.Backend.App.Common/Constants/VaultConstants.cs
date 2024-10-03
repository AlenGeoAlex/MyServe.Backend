namespace MyServe.Backend.Common.Constants;

public static class VaultConstants
{
    public static class Authentication
    {
        public const string AudienceKey = "authentication-audience";
        public const string IssuerKey = "authentication-issuer";
        public const string IssuerSigningKey = "authentication-issuer-singing-key";
    }

    public static class Supabase
    {
        public const string ProjectUrl = "supabase-project-url";
        public const string ProjectKey = "supabase-project-key";
    }

    public static class Redis
    {
        public const string RedisConnectionString = "redis-connection-string";
    }

    public static class Database
    {
        public const string ReadOnlyDatabaseName = "database-read-only";
        public const string ReadWriteDatabaseName = "database-read-write";
    }

    public static class Jwt
    {
        public const string EncryptionKey = "jwt-encryption-key";
        public const string SigningKey = "jwt-signing-key";
    }

    public static class Messaging
    {
        public static class RabbitMQ
        {
            public const string HostName = "messaging-rabbitmq-hostname";
            public const string Port = "messaging-rabbitmq-port";
            public const string Username = "messaging-rabbitmq-username";
            public const string Password = "messaging-rabbitmq-password";
            public const string VirtualHost = "messaging-rabbitmq-virtual-host";
        }

        public static class AmazonSQS
        {
            public const string ConnectionString = "messaging-amazonsqs-connection-string";
        }
    }

    public static class Storage
    {
        public static class S3
        {
            private const string BaseS3Region = "storage-s3-bucket";
            private const string BaseS3ConnectionUrl = "storage-s3-endpoint";
            private const string BaseS3ConnectionAccessKey = "storage-s3-access-key";
            private const string BaseS3ConnectionSecretKey = "storage-s3-secret-key";

            public static string BucketWithPrefix(string prefix)
            {
                return $"{prefix}-{BaseS3Region}";
            }

            public static string EndpointWithPrefix(string prefix)
            {
                return $"{prefix}-{BaseS3ConnectionUrl}";
            }

            public static string ConnectionAccessKeyWithPrefix(string prefix)
            {
                return $"{prefix}-{BaseS3ConnectionAccessKey}";
            }
            
            public static string ConnectionSecretKeyWithPrefix(string prefix)
            {
                return $"{prefix}-{BaseS3ConnectionSecretKey}";
            }
        }
    }
    
    public static class Smtp
    {
        public const string Host = "smtp-host";
        public const string Port = "smtp-port";
        public const string Username = "smtp-username";
        public const string Password = "smtp-password";
    }
}