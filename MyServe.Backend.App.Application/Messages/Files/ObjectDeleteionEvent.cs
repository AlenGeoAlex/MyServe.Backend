using MyServe.Backend.Common.Constants;

namespace MyServe.Backend.App.Application.Messages.Files;

public class ObjectDeleteionEvent
{
    public FileSource FileSource { get; set; }
    public Guid UserId { get; set; }
    public Dictionary<Guid, string?> Files { get; set; }
}