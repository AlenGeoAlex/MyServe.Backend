namespace MyServe.Backend.Common.Constants.EmailTemplate;

public class EmailTemplates
{
    public static readonly EmailTemplate ValidateOtp = new("validate-otp", "Login to MyServe - {{CODE}}");
}