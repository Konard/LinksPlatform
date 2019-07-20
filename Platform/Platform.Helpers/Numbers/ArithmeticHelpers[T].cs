using System;
using Platform.Reflection;
using Platform.Reflection.Sigil;

// ReSharper disable StaticFieldInGenericType

namespace Platform.Helpers.Numbers
{
    public class ArithmeticHelpers<T>
    {
        public static readonly Func<T, T, T> Add;
        public static readonly Func<T, T, T> And;
        public static readonly Func<T, T> Increment;
        public static readonly Func<T, T, T> Subtract;
        public static readonly Func<T, T> Decrement;

        static ArithmeticHelpers()
        {
            DelegateHelpers.Compile(out Add, emiter =>
            {
                if (!CachedTypeInfo<T>.IsNumeric)
                    throw new NotSupportedException();

                emiter.LoadArguments(0, 1);
                emiter.Add();
                emiter.Return();
            });

            DelegateHelpers.Compile(out And, emiter =>
            {
                if (!CachedTypeInfo<T>.IsNumeric)
                    throw new NotSupportedException();

                emiter.LoadArguments(0, 1);
                emiter.And();
                emiter.Return();
            });

            DelegateHelpers.Compile(out Increment, emiter =>
            {
                if (!CachedTypeInfo<T>.IsNumeric)
                    throw new NotSupportedException();

                emiter.LoadArgument(0);
                emiter.Increment(typeof(T));
                emiter.Return();
            });

            DelegateHelpers.Compile(out Subtract, emiter =>
            {
                if (!CachedTypeInfo<T>.IsNumeric)
                    throw new NotSupportedException();

                emiter.LoadArguments(0, 1);
                emiter.Subtract();
                emiter.Return();
            });

            DelegateHelpers.Compile(out Decrement, emiter =>
            {
                if (!CachedTypeInfo<T>.IsNumeric)
                    throw new NotSupportedException();

                emiter.LoadArgument(0);
                emiter.Decrement(typeof(T));
                emiter.Return();
            });
        }
    }
}
