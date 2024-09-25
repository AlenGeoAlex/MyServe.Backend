using System.Net;

namespace MyServe.Backend.App.Domain.Exceptions;

public abstract class AppException(
    string? message,
    long errorCode,
    HttpStatusCode statusCode,
    string? extendedMessage = null)
    : Exception(message)
{
    public readonly long ErrorCode = errorCode;
    public readonly HttpStatusCode StatusCode = statusCode;
    public readonly List<object> Errors = new List<object>();
    public string PublicMessage = message ?? String.Empty;
    public readonly string? ExtendedMessage = extendedMessage;

    public AppException AddError(object error)
    {
        Errors.Add(error);
        return this;
    }

    public AppException SetMessage(string? exceptionPublicMessage)
    {
        if (exceptionPublicMessage == null)
            return this;
        
        PublicMessage = exceptionPublicMessage;
        return this;
    }
}