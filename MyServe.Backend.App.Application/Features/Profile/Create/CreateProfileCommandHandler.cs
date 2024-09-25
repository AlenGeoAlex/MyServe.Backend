using MediatR;
using MyServe.Backend.App.Application.Services;
using MyServe.Backend.App.Domain.Abstracts;

namespace MyServe.Backend.App.Application.Features.Profile.Create;

public class CreateProfileCommandHandler(IReadWriteUnitOfWork unitOfWork, IProfileService profileService) : IRequestHandler<CreateProfileCommand, CreateProfileResponse>
{
    public async Task<CreateProfileResponse> Handle(CreateProfileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await using var uow = unitOfWork;
            var newProfile = await profileService.CreateNewProfile(request, cancellationToken);
            await uow.CommitAsync();
            return new CreateProfileResponse()
            {
                Id = newProfile.Id,
                Response = CreateProfileResponse.Success
            };
        }
        catch (Exception e)
        {
            await unitOfWork.DisposeAsync();
            return new CreateProfileResponse()
            {
                Id = Guid.Empty,
                Response = e.Message.Contains("duplicate") ? CreateProfileResponse.Duplicate : CreateProfileResponse.Failed
            };
        }
    }
}