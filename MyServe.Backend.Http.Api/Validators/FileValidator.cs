using FluentValidation;
using MyServe.Backend.App.Application.Features.Files.Create;
using MyServe.Backend.App.Application.Features.Files.Signed;
using MyServe.Backend.App.Domain.Models.Files;
using MyServe.Backend.Common.Constants;

namespace MyServe.Backend.Api.Validators;

class FileValidator : AbstractValidator<CreateFileCommand>
{
    public FileValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100);
        
        RuleFor(x => x.Type).IsEnumName(typeof(FileType));
        
        RuleFor(x => x.TargetUrl)
            .NotEmpty()
            .Must(x => x == null || x.StartsWith("https://"))
            .When((x) => x.Type.Equals(FileType.Obj.ToString(), StringComparison.OrdinalIgnoreCase));
        
        RuleFor(x => x.MimeType)
            .Empty()
            .When(x => x.Type.Equals(FileType.Dir.ToString(), StringComparison.OrdinalIgnoreCase));
    }
}

class FilePreSignedUrlValidator : AbstractValidator<CreateSignedUrlCommand>
{
    public FilePreSignedUrlValidator()
    {
        RuleFor(x => x.FileName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100);
        
        RuleFor(x => x.Source).IsEnumName(typeof(PublicSignedUrlRequestType));
        RuleFor(x => x.DurationInMinutes)
            .GreaterThan(0);
        
        // Request to upload profile image must be <= 3 mins in duration
        RuleFor(x => x.DurationInMinutes)
            .Must(x => x < 3)
            .When(x => PublicSignedUrlRequestType.Profile.ToString()
                .Equals(x.Source, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Signed Url's for profile will not be allowed to be greater than 3 minutes");

        // Request to upload my files should be <= 60 mins
        RuleFor(x => x.DurationInMinutes)
            .Must(x => x <= 60)
            .When(x => PublicSignedUrlRequestType.Files.ToString().Equals(x.Source, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Signed Url's for files will not be allowed to be greater than 60 minutes");
        
        // Request to upload custom files should be <= 15 mins
        RuleFor(x => x.DurationInMinutes)
            .Must(x => x <= 15)
            .When(x => PublicSignedUrlRequestType.PublicFile.ToString().Equals(x.Source, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Signed Url's for files will not be allowed to be greater than 15 minutes");
        
    }
}