using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Platform.Helpers.Reflection
{
    public abstract class Types
    {
        private static readonly ConcurrentDictionary<Type, Type[]> Cache = new ConcurrentDictionary<Type, Type[]>();

        protected Type[] ToArray()
        {
            var array = GetType().GetGenericArguments();

            var list = new List<Type>();
            AppendTypes(list, array);
            return list.ToArray();
        }

        private void AppendTypes(List<Type> list, Type[] array)
        {
            for (var i = 0; i < array.Length; i++)
            {
                var element = array[i];

                if (element == typeof(Types))
                    continue;

                if (element.IsSubclassOf(typeof(Types)))
                    AppendTypes(list, element.GetFirstField().GetStaticValue<Type[]>());
                else
                    list.Add(element);
            }
        }

        public static Type[] Get<T>()
        {
            var type = typeof(T);

            return Cache.GetOrAdd(type, t =>
            {
                if (type == typeof(Types))
                    return new Type[0];
                if (type.IsSubclassOf(typeof(Types)))
                    return type.GetFirstField().GetStaticValue<Type[]>();
                return new[] { type };
            });
        }
    }

    public class Types<T> : Types
    {
        public static readonly Type[] Array = new Types<T>().ToArray();

        private Types() { }
    }

    public class Types<T1, T2> : Types
    {
        public static readonly Type[] Array = new Types<T1, T2>().ToArray();

        private Types() { }
    }

    public class Types<T1, T2, T3> : Types
    {
        public static readonly Type[] Array = new Types<T1, T2, T3>().ToArray();

        private Types() { }
    }

    public class Types<T1, T2, T3, T4> : Types
    {
        public static readonly Type[] Array = new Types<T1, T2, T3, T4>().ToArray();

        private Types() { }
    }

    public class Types<T1, T2, T3, T4, T5> : Types
    {
        public static readonly Type[] Array = new Types<T1, T2, T3, T4, T5>().ToArray();

        private Types() { }
    }

    public class Types<T1, T2, T3, T4, T5, T6> : Types
    {
        public static readonly Type[] Array = new Types<T1, T2, T3, T4, T5, T6>().ToArray();

        private Types() { }
    }

    public class Types<T1, T2, T3, T4, T5, T6, T7> : Types
    {
        public static readonly Type[] Array = new Types<T1, T2, T3, T4, T5, T6, T7>().ToArray();

        private Types() { }
    }

    public class Types<T1, T2, T3, T4, T5, T6, T7, T8> : Types
    {
        public static readonly Type[] Array = new Types<T1, T2, T3, T4, T5, T6, T7, T8>().ToArray();

        private Types() { }
    }

    public class Types<T1, T2, T3, T4, T5, T6, T7, T8, T9> : Types
    {
        public static readonly Type[] Array = new Types<T1, T2, T3, T4, T5, T6, T7, T8, T9>().ToArray();

        private Types() { }
    }

    public class Types<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : Types
    {
        public static readonly Type[] Array = new Types<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>().ToArray();

        private Types() { }
    }

    public class Types<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : Types
    {
        public static readonly Type[] Array = new Types<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>().ToArray();

        private Types() { }
    }

    public class Types<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : Types
    {
        public static readonly Type[] Array = new Types<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>().ToArray();

        private Types() { }
    }

    public class Types<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : Types
    {
        public static readonly Type[] Array = new Types<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>().ToArray();

        private Types() { }
    }

    public class Types<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : Types
    {
        public static readonly Type[] Array = new Types<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>().ToArray();

        private Types() { }
    }
}
