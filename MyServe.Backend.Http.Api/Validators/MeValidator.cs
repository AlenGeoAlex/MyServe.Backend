using FluentValidation;
using MyServe.Backend.App.Application.Features.Profile.Search;

namespace MyServe.Backend.Api.Validators;

public class MeSearchQueryValidator : AbstractValidator<MeSearchQuery>
{

    public MeSearchQueryValidator()
    {
        RuleFor(x => x.Search)
            .MinimumLength(3)
            .WithMessage("Search length must be between 3");
    }
    
}