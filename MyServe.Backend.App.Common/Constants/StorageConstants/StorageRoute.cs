namespace MyServe.Backend.Common.Constants.StorageConstants;

public record StorageRoute(params string[]? PathSegments)
{
    public string[] Frame(params string[] fileIdentifier)
    {
        return [..PathSegments ?? [], ..fileIdentifier];
    }
}