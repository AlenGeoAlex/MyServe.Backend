using MyServe.Backend.App.Application.Abstract;
using MyServe.Backend.Common.Options;

namespace MyServe.Backend.App.Application.Features.Files.List;

public class ListFileOptions : ListOptions, IAppRequest<ListFileResponse>
{
    protected override HashSet<string> Columns => ["name", "target_size", "created_at", "type"];
    protected override string DefaultColumn => "name";
    
    public Guid? ParentId { get; set; }
    public Guid OwnerId { get; set; }
}