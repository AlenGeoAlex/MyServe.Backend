namespace MyServe.Backend.App.Application.Dto.Otp;

public class OtpDto
{
    public Guid UserId { get; set; }
    public string Otp { get; set; }
    public string Message { get; set; }
    public string EmailAddress { get; set; }
}