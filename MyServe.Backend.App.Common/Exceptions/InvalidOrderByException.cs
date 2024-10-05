namespace MyServe.Backend.Common.Exceptions;

public class InvalidOrderByException(string providedOrder, HashSet<string> columns) : Exception($"{providedOrder} is not a valid sort by field out of {string.Join(", ", columns)}")
{
    
}