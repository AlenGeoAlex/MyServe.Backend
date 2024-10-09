using MyServe.Backend.App.Application.Abstract;

namespace MyServe.Backend.App.Application.Features.Files.ById;

public class GetFileByIdQuery : IAppRequest<GetFileByIdResponse?>
{
    public required Guid Id { get; init; }
}