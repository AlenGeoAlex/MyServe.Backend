using MyServe.Backend.App.Application.Abstract;

namespace MyServe.Backend.App.Application.Features.Auth.ValidateOtp;

public class ValidateOtpCommand : IAppRequest<ValidateOtpResponse>
{
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Device { get; set; } = null!;
    public string Origin { get; set; } = null!;
}