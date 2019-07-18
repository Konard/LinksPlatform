using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Platform.Reflection;
using Platform.Helpers.Reflection.Sigil;

namespace Platform.Helpers.Unsafe
{
    public static class IntPtrHelpers<T>
    {
        public static readonly Func<IntPtr, T> GetValue;
        public static readonly Action<IntPtr, T> SetValue;

        static IntPtrHelpers()
        {
            DelegateHelpers.Compile(out GetValue, emiter =>
            {
                if (CachedTypeInfo<T>.IsNumeric)
                {
                    emiter.LoadArgument(0);
                    emiter.LoadIndirect<T>();
                    emiter.Return();
                }
                else
                {
                    emiter.LoadArguments(0);
                    emiter.Call(typeof(Marshal).GetGenericMethod("PtrToStructure", new[] { typeof(T) }, new[] { typeof(IntPtr), typeof(Type), typeof(bool) }));
                    emiter.Return();
                }
            });

            DelegateHelpers.Compile(out SetValue, emiter =>
            {
                if (CachedTypeInfo<T>.IsNumeric)
                {
                    emiter.LoadArguments(0, 1);
                    emiter.StoreIndirect<T>();
                    emiter.Return();
                }
                else
                {
                    emiter.LoadArguments(0, 1);
                    emiter.LoadConstant(true);
                    emiter.Call(typeof(Marshal).GetTypeInfo().GetMethod("StructureToPtr", new[] { typeof(object), typeof(IntPtr), typeof(bool) }));
                    emiter.Return();
                }
            });
        }
    }
}
