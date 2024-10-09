using System.Net;
using MyServe.Backend.App.Domain.Models.Profile;
using File = MyServe.Backend.App.Domain.Models.Files.File;

namespace MyServe.Backend.App.Domain.Exceptions;

public class DataWriteFailedException(Type entityType, string stringToMatch, Exception? e = null) : Exception(stringToMatch, e)
{
    public ConstraintInfo ConstraintInfo { get; } = MatchError(entityType, stringToMatch);

    private static readonly Dictionary<string, List<ConstraintInfo>> Constraints =
        new Dictionary<string, List<ConstraintInfo>>
        {
            {
                nameof(File),
                [
                    new ConstraintInfo("fk_file_parent", "The parent directory of the file does not exist.", HttpStatusCode.InternalServerError),
                    new ConstraintInfo("fk_file_created", "The reference for the file created could not be found.",HttpStatusCode.InternalServerError),
                    new ConstraintInfo("fk_file_owner", "The reference for the file owner could not be found.", HttpStatusCode.InternalServerError),
                    new ConstraintInfo("unq_file_name_parent_owner", "A file/directory with the name already exists!", HttpStatusCode.Conflict),
                    new ConstraintInfo("Parent is not a directory", "The provided parent directory reference is not a valid directory.", HttpStatusCode.InternalServerError),
                    new ConstraintInfo("No permission in parent", "You are not the owner of the parent directory.", HttpStatusCode.Forbidden),
                    new ConstraintInfo("Parent file is soft deleted", "Parent field is marked to be deleted.", HttpStatusCode.InternalServerError),
                ]
            },
            {
                nameof(Profile),
                [
                ]
            }
        };

    private static ConstraintInfo MatchError(Type entityType, string stringToMatch)
    {
        if (!Constraints.TryGetValue(entityType.Name, out var constraintInfos))
            return ConstraintInfo.Empty;

        foreach (var constraintInfo in constraintInfos.Where(constraintInfo => stringToMatch.Contains(constraintInfo.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return constraintInfo;
        }
        
        return new ConstraintInfo(entityType.Name, stringToMatch, HttpStatusCode.InternalServerError);
    }
}

public class ConstraintInfo(string name, string errorMessage, HttpStatusCode code)
{
    public static readonly ConstraintInfo Empty = new ("Unknown", "An unknown constraint validation failed", HttpStatusCode.InternalServerError);
    
    public string Name { get; private set; } = name;
    public string ErrorMessage { get; private set; } = errorMessage;
    public HttpStatusCode HttpStatusCode { get; private set; } = code;
}