using System;
using System.Reflection;
using Platform.Reflection;
using Platform.Reflection.Sigil;

// ReSharper disable StaticFieldInGenericType

namespace Platform.Helpers.Numbers
{
    public static class MathHelpers<T>
    {
        /// <remarks>
        /// TODO: Возможно, если компиляцию производить не в делегат, а в объект с методом, то оптимизация компилятором будет работать лучше.
        /// TODO: Возможно использовать ссылку на существующий метод - оператор, а не компилировать новый (http://stackoverflow.com/questions/11113259/how-to-call-custom-operator-with-reflection)
        /// TODO: Решить что лучше dynamic operator или Auto&lt;T&gt; c заранее созданными операторами и возможность расширения через статические методы
        /// </remarks>
        public static readonly Func<T, T> Abs;
        public static readonly Func<T, T> Negate;

        static MathHelpers()
        {
            DelegateHelpers.Compile(out Abs, emiter =>
            {
                if (!CachedTypeInfo<T>.IsNumeric)
                    throw new NotSupportedException();

                emiter.LoadArgument(0);

                if (CachedTypeInfo<T>.IsSigned)
                    emiter.Call(typeof(Math).GetTypeInfo().GetMethod("Abs", new[] { typeof(T) }));

                emiter.Return();
            });

            DelegateHelpers.Compile(out Negate, emiter =>
            {
                if (!CachedTypeInfo<T>.IsNumeric || !CachedTypeInfo<T>.IsSigned)
                    throw new NotSupportedException();

                emiter.LoadArgument(0);
                emiter.Negate();
                emiter.Return();
            });
        }
    }
}
