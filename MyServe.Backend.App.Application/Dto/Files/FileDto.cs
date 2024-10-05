using MyServe.Backend.App.Application.Dto.User;
using MyServe.Backend.App.Domain.Models.Files;

namespace MyServe.Backend.App.Application.Dto.Files;

public class FileDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid? ParentId { get; set; }
    public FileType Type { get; set; }
    public Guid Owner { get; set; }
    public UserIdentificationDto OwnerProfile { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid Created { get; set; }
    public string? TargetUrl { get; set; }
    public long TargetSize { get; set; }
    public string? MimeType { get; set; }    
    public List<object> AccessControl { get; } = [];
    public FileDto Parent { get; set; }
}