using FluentValidation;
using Microsoft.Extensions.Primitives;
using MyServe.Backend.App.Application.Features.Auth.CreateOtp;
using MyServe.Backend.App.Application.Features.Auth.OAuth;
using MyServe.Backend.App.Application.Features.Auth.ValidateOtp;
using MyServe.Backend.Common.Constants;

namespace MyServe.Backend.Api.Validators;

public class OtpValidator : AbstractValidator<ValidateOtpCommand>
{
    public OtpValidator(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .NotNull();
        
        RuleFor(x => x.Code)
            .Length(6)
            .WithMessage("Code must be 6 characters");

        List<string> origins = [];
        configuration.Bind("Origins", origins);

        var origin = httpContextAccessor.HttpContext?.Request.Headers.Origin ?? StringValues.Empty;
        RuleFor(x => x)
            .Must(x => Validator.ValidateOriginFromHeaders(origin, origins))
            .WithMessage("Failed to validate the authenticity of the request");
    }
}

public class OtpRequestValidator : AbstractValidator<CreateOtpCommand>
{
    public OtpRequestValidator(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        RuleFor(x => x.EmailAddress)
            .NotNull()
            .NotEmpty()
            .EmailAddress();
        
        List<string> origins = [];
        configuration.Bind("Origins", origins);

        var origin = httpContextAccessor.HttpContext?.Request.Headers.Origin ?? StringValues.Empty;
        RuleFor(x => x)
            .Must(x => Validator.ValidateOriginFromHeaders(origin, origins))
            .WithMessage("Failed to validate the authenticity of the request");
    }
}

public class OAuthValidator : AbstractValidator<OAuthCommand>
{
    public OAuthValidator(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        RuleFor(x => x.Identity)
            .NotEmpty()
            .NotNull();

        RuleFor(x => x.OAuthType)
            .IsEnumName(typeof(OAuthTypes));
        
        List<string> origins = [];
        configuration.Bind("Origins", origins);

        var origin = httpContextAccessor.HttpContext?.Request.Headers.Origin ?? StringValues.Empty;
        RuleFor(x => x)
            .Must(x => Validator.ValidateOriginFromHeaders(origin, origins))
            .WithMessage("Failed to validate the authenticity of the request");
    }
}

static class Validator
{
    public static bool ValidateOriginFromHeaders(StringValues origin, List<string> origins)
    {
        return origin != StringValues.Empty && origins.Contains(origin.ToString());
    } 
} 