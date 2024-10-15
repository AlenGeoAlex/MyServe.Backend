using MyServe.Backend.Common.Constants;

namespace MyServe.Backend.App.Application.Dto.Me;

public class SearchDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Service { get; set; }
    public Dictionary<string, string?> Metadata { get; set; } = new();
}