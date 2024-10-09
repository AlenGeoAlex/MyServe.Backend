using System.Net;

namespace MyServe.Backend.App.Domain.Exceptions.File;

public class InvalidFileException(string message) : AppException(message, 2142, HttpStatusCode.InternalServerError) { }