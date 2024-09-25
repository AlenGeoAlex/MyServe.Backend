using FluentValidation;
using FluentValidation.Results;
using MediatR;
using MyServe.Backend.App.Application.Abstract;

namespace MyServe.Backend.App.Application.Behaviours;

public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequestBase
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        ValidationResult[] results = await Task.WhenAll(validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));
        
        var errors = results 
            .Where(result => !result.IsValid)
            .SelectMany(result => result.Errors)
            .Select(errors => new ValidationFailure(
                errors.PropertyName,
                errors.ErrorMessage
            ));

        var validationErrors = errors.ToList();
        if (validationErrors.Count != 0)
        {
            throw new ValidationException(validationErrors);
        }

        var response = await next();
        return response;
    }
}

