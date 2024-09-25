using System.Net;

namespace MyServe.Backend.App.Domain.Exceptions;

public class UnknownException(string? message, long errorCode, HttpStatusCode statusCode, string? extendedMessage = null) : AppException(message, errorCode, statusCode, extendedMessage);