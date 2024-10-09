using System.Net;

namespace MyServe.Backend.App.Domain.Exceptions.File;

public class InvalidDirectoryAccessException(string directoryName) : AppException($"Directory {directoryName} can't be directly accessed using id endpoint!", 011, HttpStatusCode.Forbidden, null)
{
    
}