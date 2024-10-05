using MyServe.Backend.App.Application.Abstract;
using MyServe.Backend.App.Domain.Models.Files;

namespace MyServe.Backend.App.Application.Features.Files.Create;

public class CreateFileCommand : IAppRequest<CreateFileResponse>
{
    public string Name { get; set; }
    public Guid? ParentId { get; set; }
    public string Type { get; set; }
    public string? TargetUrl { get; set; }
    public long TargetSize { get; set; }
    public string? MimeType { get; set; }
    public Guid Owner { get; set; }
}