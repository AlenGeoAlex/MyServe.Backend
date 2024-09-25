namespace MyServe.Backend.Common.Constants;

public static class EmailPlaceholdersConstants
{
    public static class Subject
    {
        public static class ValidateOtp
        {
            public const string Code = "CODE";
        }
    }

    public static class Body
    {
        public static class ValidateOtp
        {
            public const string Code = "CODE";
            public const string User = "USER";
            public const string Device = "DEVICE";
            public const string Url = "URL";
        }
    }
}