namespace MyServe.Backend.Api.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class NoCacheWipeAttribute : Attribute
{
    
}