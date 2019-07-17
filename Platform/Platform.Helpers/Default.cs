using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Platform.Helpers
{
    /// <summary>
    /// Представляет собой точку доступа к экземплярям типов по умолчанию (созданных с помощью конструктора без аргументов).
    /// </summary>
    /// <typeparam name="T">Тип экземпляра объекта.</typeparam>
    public class Default<T>
        where T : new()
    {
        /// <summary>
        /// Default.GetOrCreateThreadInstance methods are recommended instead.
        /// Use direclty only if you understand what you are doing (see remarks for hint).
        /// </summary>
        /// <remarks>
        /// If you really need maximum performance, use this field together with Default.InitializeThreadInstance method.
        /// This method should be called only once per thread, but you can call multiple times if you need to replace the instance with new one.
        /// </remarks>
        [ThreadStatic]
        public static T ThreadInstance;

        public static readonly T Instance = new T();
    }

    public class Default
    {
        public static void InitializeThreadInstance<T>()
            where T : new() => Default<T>.ThreadInstance = new T();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrCreateThreadInstance<T>()
            where T : class, new() => Default<T>.ThreadInstance ?? (Default<T>.ThreadInstance = new T());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T GetOrCreateThreadInstance<T>(T nullValue)
            where T : struct => EqualityComparer<T>.Default.Equals(Default<T>.ThreadInstance, nullValue) ? (Default<T>.ThreadInstance = new T()) : Default<T>.ThreadInstance;
    }
}
