using System.Net;
using MediatR;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Abstracts;
using MyServe.Backend.App.Domain.Exceptions;

namespace MyServe.Backend.App.Application.Features.Files.Create;

public class CreateFileCommandHandler(IFileService fileService, IReadWriteUnitOfWork readWriteUnitOfWork) : IRequestHandler<CreateFileCommand, CreateFileResponse>
{
    public async Task<CreateFileResponse> Handle(CreateFileCommand request, CancellationToken cancellationToken)
    {
        await using var uow = await readWriteUnitOfWork.StartTransactionAsync();
        try
        {
            var response = await fileService.Create(request);
            await uow.CommitAsync();
        }
        catch (Exception e)
        {
            await uow.RollbackAsync();
            if (e is DataWriteFailedException { ConstraintInfo.HttpStatusCode: HttpStatusCode.Conflict })
                return new CreateFileResponse()
                {
                    Message = CreateFileResponse.Duplicate
                };
        }
        return new CreateFileResponse();
    }
}