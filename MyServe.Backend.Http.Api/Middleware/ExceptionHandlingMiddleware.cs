using System.Text.Json.Serialization;
using FluentValidation;
using MyServe.Backend.Common.Extensions;
using MyServe.Backend.Common.Impl;
using MyServe.Backend.App.Domain.Exceptions;

namespace MyServe.Backend.Api.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IWebHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AggregateException aggEx)
        {
            var innerException = aggEx.Flatten().InnerException;
            logger.LogError(innerException, "An aggregate exception has been caught on the request pipeline.");
            await HandleExceptionAsync(context, innerException);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception has been caught on the request pipeline.");
            await HandleExceptionAsync(context, ex);
        }
    }
    
    private async Task HandleExceptionAsync(HttpContext context, Exception? exception)
    {
        ExceptionDetail exceptionDetail = BuildExceptionMessage(exception, env.IsDevelopment());
        context.Response.StatusCode = exceptionDetail.StatusCode;
        context.Response.ContentType = "application/json";
        if (exceptionDetail.StatusCode is < 500 and >= 300 )
        {
            context.Response.Headers["MySe-Code"] = exceptionDetail.ErrorCode.ToString();
            context.Response.Headers["MySe-Message"] = exceptionDetail.Message;
        }

        if (exceptionDetail is { StatusCode: 401, Type: "Access Token Validation" })
        {
            context.Response.Headers["MySe-ExpiredAccess"] = "true";
        }
        await context.Response.WriteAsJsonAsync(exceptionDetail);
    }
    
    
    private static ExceptionDetail BuildExceptionMessage(Exception? exception, bool showExtended = false)
    {
        return exception switch
        {
            ValidationException validationException => new ExceptionDetail(
                StatusCodes.Status400BadRequest,
                "Validation Error",
                "Failed to validate properties provided",
                validationException.Errors.GroupBy(x => x.PropertyName).ToHashSet(),
                ErrorCode: 400
            ),
            // JwtGenerationException jwtGenerationException => new ExceptionDetail(
            //     StatusCodes.Status500InternalServerError,
            //     "Access Generation Failure",
            //     $"Failed to generate access tokens.",
            //     null,
            //     ExtendedMessage: showExtended ? jwtGenerationException.Message : ""
            //     ),
            // JwtFetchKeySecretException jwtFetchKeySecretException => new ExceptionDetail(
            //     StatusCodes.Status500InternalServerError,
            //     "Access Generation Failure",
            //     $"Failed to fetch keys. {jwtFetchKeySecretException.Message}",
            //     null,
            //     ExtendedMessage: showExtended ? jwtFetchKeySecretException.Message : ""
            //     ),
            UnauthorizedAccessException unauthorizedAccessException => unauthorizedAccessException.Message.Contains("Lifetime validation failed. The token is expired") 
                ? new ExceptionDetail(
                StatusCodes.Status401Unauthorized,
                "Access Token Validation",
                "Access token validity has been expired",
                null,
                null)
                : new ExceptionDetail(
                    StatusCodes.Status403Forbidden,
                    "Forbidden access",
                    unauthorizedAccessException.Message,
                    null,
                    null),
            BadHttpRequestException badHttpRequestException => new ExceptionDetail(
                StatusCodes.Status400BadRequest,
                Type: "The request is not valid",
                Message: badHttpRequestException.Message,
                Errors: null,
                ExtendedMessage: showExtended ? badHttpRequestException.InnerException?.Message : ""
                ),
            AppException appException => CreateExceptionDetail(appException),
            _ => new ExceptionDetail(
                StatusCodes.Status500InternalServerError,
                "Unknown Error",
                "An unknown error occured. Please retry again",
                null
            )
        };
    }
    
    private static ExceptionDetail CreateExceptionDetail(AppException appException)
    {
        var typeName = appException.GetType().Name;
        var readableName = ExceptionCache.Exceptions.GetOrAdd(typeName, key =>
            key.ToCapitalized().Replace("Exception", string.Empty).TrimEnd()
        );
        

        return new ExceptionDetail(
            (int)appException.StatusCode,
            readableName,
            appException.PublicMessage == String.Empty ? appException.Message : appException.PublicMessage,
            appException.Errors, 
            appException.ExtendedMessage ?? appException.InnerException?.Message,
            ErrorCode: appException.ErrorCode
        );
    }
}


internal sealed record ExceptionDetail(
    [property: JsonIgnore] [field: JsonIgnore] int StatusCode,
    string Type, string Message, 
    IEnumerable<object>? Errors, 
    string? ExtendedMessage = null,
    long? ErrorCode = null
    );