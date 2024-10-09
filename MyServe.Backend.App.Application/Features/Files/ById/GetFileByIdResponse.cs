using MyServe.Backend.App.Application.Dto.Files;

namespace MyServe.Backend.App.Application.Features.Files.ById;

public class GetFileByIdResponse
{
    public FileDto File { get; set; }
    public DateTimeOffset Expiry { get; set; }
}