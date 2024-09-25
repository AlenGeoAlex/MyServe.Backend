using FluentValidation;

namespace MyServe.Backend.App.Application.Features.Profile.Me;

public class MeQueryValidator : AbstractValidator<MeByIdQuery>
{
    public MeQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEqual(Guid.Empty);
    }
}