using System.Collections.Concurrent;

namespace MyServe.Backend.Common.Impl;

public class ExceptionCache
{
    public static readonly ConcurrentDictionary<string, string> Exceptions = new();
}