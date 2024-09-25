using MyServe.Backend.App.Application.Abstract;

namespace MyServe.Backend.App.Application.Features.Profile.Me;

public record MeByIdQuery(Guid UserId) : IAppRequest<MeResponse?>;