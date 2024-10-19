namespace MyServe.Backend.App.Application.Dto.Profile;

public class ProfileDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public ProfileSettingDto ProfileSettings { get; set; }
    public string? ProfileImageUrl { get; set; }

}
