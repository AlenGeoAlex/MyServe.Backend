using MyServe.Backend.Common.Constants;

namespace MyServe.Backend.Common.Exceptions;

public class InfrastructureException(InfrastructureSource infrastructureSource, string message ,Exception? exception = default) : Exception(message, exception)
{
    
}