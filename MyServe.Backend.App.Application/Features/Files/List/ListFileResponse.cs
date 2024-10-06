using MyServe.Backend.App.Application.Dto.Files;

namespace MyServe.Backend.App.Application.Features.Files.List;

public class ListFileResponse
{
    public List<FileDto> Files { get; set; } = new List<FileDto>();
    public List<FileDto> Parents { get; set; } = new List<FileDto>();
}