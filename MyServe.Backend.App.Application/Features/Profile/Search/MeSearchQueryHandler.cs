using MediatR;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Abstracts;

namespace MyServe.Backend.App.Application.Features.Profile.Search;

public class MeSearchQueryHandler(
    IProfileService profileService,
    IReadOnlyUnitOfWork readOnlyUnitOfWork
    ) : IRequestHandler<MeSearchQuery, MeSearchResponse>
{
    public async Task<MeSearchResponse> Handle(MeSearchQuery request, CancellationToken cancellationToken)
    {
        await using var uow = await readOnlyUnitOfWork.StartTransactionAsync();
        try
        {
            var matchedList = await profileService.SearchAsync(request, cancellationToken);
            return new MeSearchResponse()
            {
                Matched = matchedList
            };
        }
        catch (Exception e)
        {
            await uow.RollbackAsync();
            return new MeSearchResponse();
        }
    }
}