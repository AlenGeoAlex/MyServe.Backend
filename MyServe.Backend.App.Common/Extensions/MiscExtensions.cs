using System.Dynamic;

namespace MyServe.Backend.Common.Extensions;

public static class MiscExtensions
{
    public static bool DoesPropertyExist(dynamic dynamicObject, string name)
    {
        if (dynamicObject is ExpandoObject or IDictionary<object, object>)
            return ((IDictionary<string, object>)dynamicObject).ContainsKey(name);

        return dynamicObject.GetType().GetProperty(name) != null;
    }
}