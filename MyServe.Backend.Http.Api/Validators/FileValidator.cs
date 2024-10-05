using FluentValidation;
using MyServe.Backend.App.Application.Features.Files.Create;
using MyServe.Backend.App.Domain.Models.Files;

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