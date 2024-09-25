using MyServe.Backend.App.Application.Abstract;

namespace MyServe.Backend.App.Application.Features.Auth.Refresh;

public class RefreshTokenCommand : IAppRequest<RefreshTokenResponse?>
{
    public Guid TokenId { get; set; }

    public string Device { get; set; } = "WebApp";
}