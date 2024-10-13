using MyServe.Backend.App.Application.Abstract;

namespace MyServe.Backend.App.Application.Features.Auth.OAuth;

public class OAuthCommand : IAppRequest<OAuthResponse>
{
    public string Identity { get; set; }
    public string OAuthType { get; set; }
    public string Device { get; set; } = "WebApp";
    public string Origin { get; set; }
}