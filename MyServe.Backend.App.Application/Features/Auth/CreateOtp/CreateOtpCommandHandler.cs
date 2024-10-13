using MediatR;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Abstracts;

namespace MyServe.Backend.App.Application.Features.Auth.CreateOtp;

public class CreateOtpCommandHandler(IReadWriteUnitOfWork readWriteUnitOfWork, IUserOtpService userOtpService) : IRequestHandler<CreateOtpCommand, CreateOtpResponse>
{
    public async Task<CreateOtpResponse> Handle(CreateOtpCommand request, CancellationToken cancellationToken)
    {
        await using var uow = await readWriteUnitOfWork.StartTransactionAsync();
        try
        {
            var otp = await userOtpService.CreateUserOtpAsync(request.EmailAddress,request.Origin, request.Device);
            await uow.CommitAsync();
            return new CreateOtpResponse()
            {
                Success = otp.UserId != Guid.Empty,
                Message = otp.UserId == Guid.Empty ? string.Empty : otp.Message,
            };

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}