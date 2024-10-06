namespace MyServe.Backend.App.Application.Features.Files.Signed;

public class CreateSignedUrlResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Uri? AccessUrl { get; set; }
}