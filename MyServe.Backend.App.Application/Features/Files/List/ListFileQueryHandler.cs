using MediatR;
using MyServe.Backend.App.Application.Services;

namespace MyServe.Backend.App.Application.Features.Files.List;

public class ListFileQueryHandler(IFileService fileService) : IRequestHandler<ListFileOptions, ListFileResponse>
{
    public async Task<ListFileResponse> Handle(ListFileOptions request, CancellationToken cancellationToken)
    {
        var listMyFiles = await fileService.ListMyFiles(request.OwnerId, request.ParentId, request);

        return new ListFileResponse()
        {
            Files = listMyFiles
        };
    }
}