namespace MyServe.Backend.App.Domain.Models.Files;

public class File
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid? ParentId { get; set; }
    public File? Parent { get; set; }
    public FileType Type { get; set; }
    public string? TargetUrl { get; set; }
    public long TargetSize { get; set; }
    public string? MimeType { get; set; }
    public Guid Owner { get; set; }
    public Profile.Profile? OwnerProfile { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid Created { get; set; }
}