using MyServe.Backend.App.Domain.Models.Files;

namespace MyServe.Backend.App.Domain.Extensions;

public static class EnumExtensions
{
    public static string GetFileTypeAsString(this FileType fileType)
    {
        return fileType.ToString().ToLower();
    }

    public static FileType? GetFileTypeFromString(this string fileType)
    {
        return fileType.ToLower() switch
        {
            "dir" => FileType.Dir,
            "obj" => FileType.Obj,
            "meta" => FileType.Meta,
            _ => null
        };
    }
}