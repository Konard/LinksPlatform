using System.Collections.Generic;
using System.Dynamic;

namespace Platform.Helpers
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
