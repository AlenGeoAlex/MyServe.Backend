using MyServe.Backend.App.Application.Abstract;

namespace MyServe.Backend.App.Application.Features.Profile.Create;

public class CreateProfileCommand : IAppRequest<CreateProfileResponse>
{
    public Guid ProfileId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string EncryptionKey { get; set; }
    public CreateProfileSettingsCommand Settings { get; set; } = new();
    public Stream? ProfileImageStream = null!;
    public string? ContentType = null!;
}

public class CreateProfileSettingsCommand
{
    public bool NotificationEnabled { get; set; } = true;
}