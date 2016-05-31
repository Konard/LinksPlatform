using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace Platform.Helpers.Reflection
{
    public static class DynamicExtensions
    {
        public static bool HasProperty(this object @object, string propertyName)
        {
            var type = @object.GetType();
            if (type == typeof(ExpandoObject))
                return ((IDictionary<string, object>)@object).ContainsKey(propertyName);
            return type.GetProperty(propertyName) != null;
        }
    }
}
