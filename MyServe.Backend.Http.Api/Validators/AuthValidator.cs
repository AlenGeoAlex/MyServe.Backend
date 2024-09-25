using FluentValidation;
using MyServe.Backend.App.Application.Features.Auth.ValidateOtp;

namespace MyServe.Backend.Api.Validators;

public class AuthValidator : AbstractValidator<ValidateOtpCommand>
{
    public AuthValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .NotNull();
        
        RuleFor(x => x.Code)
            .Length(6)
            .WithMessage("Code must be 6 characters");
    }
}