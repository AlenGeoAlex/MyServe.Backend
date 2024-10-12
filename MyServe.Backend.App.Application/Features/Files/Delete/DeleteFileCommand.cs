using MyServe.Backend.App.Application.Abstract;

namespace MyServe.Backend.App.Application.Features.Files.Delete;

public class DeleteFileCommand : IAppRequest<DeleteFileResponse>
{
    public Guid Id { get; set; }
}