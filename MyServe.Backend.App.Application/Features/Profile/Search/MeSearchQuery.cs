using MyServe.Backend.App.Application.Abstract;

namespace MyServe.Backend.App.Application.Features.Profile.Search;

public class MeSearchQuery : IAppRequest<MeSearchResponse>
{
    public Guid UserId { get; set; }
    
    public string Search { get; set; }
}