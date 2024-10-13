using MyServe.Backend.App.Application.Abstract;

namespace MyServe.Backend.App.Application.Features.Auth.CreateOtp;

public class CreateOtpCommand : IAppRequest<CreateOtpResponse>
{
    public string EmailAddress { get; set; }
    public string Device { get; set; }
    public string Origin { get; set; }
}