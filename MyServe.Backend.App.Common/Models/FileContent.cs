using System.Net.Mime;

namespace MyServe.Backend.Common.Models;

public class FileContent
{
    public string? FileName { get; init; }
    public required Stream FileStream { get; init; }
    public ContentType? ContentType { get; init; }
}