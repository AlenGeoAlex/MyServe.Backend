using MyServe.Backend.App.Application.Abstract;
using MyServe.Backend.Common.Constants;

namespace MyServe.Backend.App.Application.Features.Files.Signed;

public class CreateSignedUrlCommand : IAppRequest<CreateSignedUrlResponse>
{
    public string FileName { get; set; } = null!;
    public string Source { get; set; }
    public long DurationInMinutes { get; set; }

    public TimeSpan Duration => TimeSpan.FromMinutes(DurationInMinutes);
    public PublicSignedUrlRequestType SourceParsed => Enum.Parse<PublicSignedUrlRequestType>(Source, true);

    #region Specifics

    // For files
    public Guid? ParentId { get; set; }

    #endregion
}