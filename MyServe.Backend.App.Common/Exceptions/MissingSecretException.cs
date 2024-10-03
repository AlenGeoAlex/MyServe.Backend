namespace MyServe.Backend.Common.Exceptions;

public class MissingSecretException(string key, Exception? exception) : Exception($"Failed to find the secret {key}. Please add it in your respective vault!", exception)
{
    
}