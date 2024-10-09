using System.Net;

namespace MyServe.Backend.App.Domain.Exceptions.File;

public class InvalidFileAccessException(string fileName) : AppException($"Not permission to access {fileName}", 291, HttpStatusCode.Forbidden, null)
{
    
}