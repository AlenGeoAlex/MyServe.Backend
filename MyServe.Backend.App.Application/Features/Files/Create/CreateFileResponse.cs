using MyServe.Backend.App.Application.Dto.Files;

namespace MyServe.Backend.App.Application.Features.Files.Create;

public class CreateFileResponse
{
   public const string Duplicate = nameof(Duplicate);
   public FileDto? File { get; set; }
   public string? Message { get; set; }
}