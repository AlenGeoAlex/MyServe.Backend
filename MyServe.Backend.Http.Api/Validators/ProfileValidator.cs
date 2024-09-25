using FluentValidation;
using MyServe.Backend.App.Application.Features.Profile.Create;

namespace MyServe.Backend.Api.Validators;

class CreateProfileValidator : AbstractValidator<CreateProfileCommand>
{
    public CreateProfileValidator()
    {
        // RuleFor(x => x.Email)
        //     .NotEmpty()
        //     .EmailAddress();
        
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50);
        
        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50);
        
    }
}