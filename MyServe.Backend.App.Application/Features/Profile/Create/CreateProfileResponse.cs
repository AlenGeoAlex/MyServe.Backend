namespace MyServe.Backend.App.Application.Features.Profile.Create;

public class CreateProfileResponse
{
    public const string Success = "success";
    public const string Duplicate = "duplicate";
    public const string Failed = "failed";
    
    public Guid Id { get; set; }
    
    public string Response { get; set; }
}