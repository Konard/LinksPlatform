using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Platform.Exceptions;
using Platform.Reflection;
using Platform.Reflection.Sigil;
using Platform.Helpers.Unsafe;

namespace Platform.Helpers
{
    /// <remarks>
    /// Shorter version of ConvertationHelpers.
    /// Укороченная версия от ConvertationHelpers.
    /// 
    /// Возможно нужно несколько разных способов разрешения конфликта.
    /// ExceptionConflictReation (выбрасывает исключение, если обнаружен конфликт, не предпринимая никаких действий)
    /// > Max: Max (если число больше его максимального размера в этом размере, то берём максимальное)
    /// > Max: 0 (если число больше его максимального размера в этом размере, то берём обнуляем)
    /// и т.п. (например определённые пользователем)
    /// 
    /// Текущая логика алгоритма "Closest"
    /// </remarks>
    public static class To
    {
        public const char UnknownCharacter = '�';

        private static class SignProcessor<T>
        {
            public static readonly Func<T, object> ToSignedFunc;
            public static readonly Func<T, object> ToUnsignedFunc;
            public static readonly Func<object, T> ToUnsignedAsFunc;

            static SignProcessor()
            {
                DelegateHelpers.Compile(out ToSignedFunc, emiter =>
                {
                    EnsureSupportedUnsignedTypes();

                    emiter.LoadArgument(0);

                    var method = typeof(To).GetTypeInfo().GetMethod("ToSigned", Types<T>.Array);

                    emiter.Call(method);

                    emiter.Box(method.ReturnType);

                    emiter.Return();
                });

                DelegateHelpers.Compile(out ToUnsignedFunc, emiter =>
                {
                    EnsureSupportedSignedTypes();

                    emiter.LoadArgument(0);

                    var method = typeof(To).GetTypeInfo().GetMethod("ToUnsigned", Types<T>.Array);

                    emiter.Call(method);

                    emiter.Box(method.ReturnType);

                    emiter.Return();
                });

                DelegateHelpers.Compile(out ToUnsignedAsFunc, emiter =>
                {
                    EnsureSupportedUnsignedTypes();

                    emiter.LoadArgument(0);

                    var signedVersion = CachedTypeInfo<T>.SignedVersion;

                    emiter.UnboxAny(signedVersion);

                    var method = typeof(To).GetTypeInfo().GetMethod("ToUnsigned", new [] { signedVersion });

                    emiter.Call(method);

                    emiter.Return();
                });
            }

            private static void EnsureSupportedUnsignedTypes()
            {
                var type = typeof(T);
                if (type != typeof(ulong) && type != typeof(uint) && type != typeof(ushort) && type != typeof(byte))
                    throw new NotSupportedException();
            }

            private static void EnsureSupportedSignedTypes()
            {
                var type = typeof(T);
                if (type != typeof(long) && type != typeof(int) && type != typeof(short) && type != typeof(sbyte))
                    throw new NotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong UInt64(ulong value) => value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Int64(ulong value) => value > long.MaxValue ? long.MaxValue : (long)value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint UInt32(ulong value) => value > uint.MaxValue ? uint.MaxValue : (uint)value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Int32(ulong value) => value > int.MaxValue ? int.MaxValue : (int)value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort UInt16(ulong value) => value > ushort.MaxValue ? ushort.MaxValue : (ushort)value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Int16(ulong value) => value > (ulong)short.MaxValue ? short.MaxValue : (short)value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Byte(ulong value) => value > byte.MaxValue ? byte.MaxValue : (byte)value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte SByte(ulong value) => value > (ulong)sbyte.MaxValue ? sbyte.MaxValue : (sbyte)value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Boolean(ulong value) => value > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char Char(ulong value) => value > char.MaxValue ? UnknownCharacter : (char)value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime DateTime(ulong value) => value > long.MaxValue ? System.DateTime.MaxValue : new DateTime((long)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan TimeSpan(ulong value) => value > long.MaxValue ? System.TimeSpan.MaxValue : new TimeSpan((long)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong UInt64(long value) => value < (long)ulong.MinValue ? ulong.MinValue : (ulong)value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong UInt64(int value) => value < (int)ulong.MinValue ? ulong.MinValue : (ulong)value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong UInt64(short value) => value < (short)ulong.MinValue ? ulong.MinValue : (ulong)value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong UInt64(sbyte value) => value < (sbyte)ulong.MinValue ? ulong.MinValue : (ulong)value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong UInt64(bool value) => value ? 1UL : 0UL;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong UInt64(char value) => value;

        public static long ToSigned(ulong value) => (long)value;

        public static int ToSigned(uint value) => (int)value;

        public static short ToSigned(ushort value) => (short)value;

        public static sbyte ToSigned(byte value) => (sbyte)value;

        public static object ToSigned<T>(T value) => SignProcessor<T>.ToSignedFunc(value);

        public static ulong ToUnsigned(long value) => (ulong)value;

        public static uint ToUnsigned(int value) => (uint)value;

        public static ushort ToUnsigned(short value) => (ushort)value;

        public static byte ToUnsigned(sbyte value) => (byte)value;

        public static object ToUnsigned<T>(T value) => SignProcessor<T>.ToUnsignedFunc(value);

        public static T ToUnsignedAs<T>(object value) => SignProcessor<T>.ToUnsignedAsFunc(value);

        
    }
}
