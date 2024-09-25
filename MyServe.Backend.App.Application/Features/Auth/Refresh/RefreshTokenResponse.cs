namespace MyServe.Backend.App.Application.Features.Auth.Refresh;

public class RefreshTokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}