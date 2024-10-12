using Microsoft.AspNetCore.JsonPatch;
using MyServe.Backend.App.Application.Abstract;
using MyServe.Backend.App.Application.Dto.Files;

namespace MyServe.Backend.App.Application.Features.Files.Patch;

public class FilePatchCommand : IAppRequest<FileDto>
{
    public Guid Id { get; set; }
    public JsonPatchDocument<FileDto> Document { get; set; }
}