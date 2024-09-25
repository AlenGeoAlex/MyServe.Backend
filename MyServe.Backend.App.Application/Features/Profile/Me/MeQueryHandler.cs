using MediatR;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Abstracts;
using Newtonsoft.Json;

namespace MyServe.Backend.App.Application.Features.Profile.Me;

public class MeQueryHandler(IProfileService service) : IRequestHandler<MeByIdQuery, MeResponse?>
{
    public async Task<MeResponse?> Handle(MeByIdQuery request, CancellationToken cancellationToken)
    {
        var profileDto = await service.GetProfileAsync(request.UserId, cancellationToken);
        if (profileDto is null)
            return null;
        
        return JsonConvert.DeserializeObject<MeResponse>(JsonConvert.SerializeObject(profileDto));
    }
}