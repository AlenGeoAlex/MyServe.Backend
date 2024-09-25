namespace MyServe.Backend.App.Application.Features.Auth.ValidateOtp;

public class ValidateOtpResponse 
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}