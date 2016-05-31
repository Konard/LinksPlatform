using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
    /// </remarks>
    public static class To
    {
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
        public static char Char(ulong value)
        {
            const char unknownCharacter = '�';
            return value > char.MaxValue ? unknownCharacter : (char)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime DateTime(ulong value) => value > long.MaxValue ? System.DateTime.MaxValue : new DateTime((long)value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TimeSpan TimeSpan(ulong value) => value > long.MaxValue ? System.TimeSpan.MaxValue : new TimeSpan((long)value);

        public static byte[] Bytes<T>(T obj)
            where T : struct
        {
            var structureSize = Marshal.SizeOf(obj);

            var bytes = new byte[structureSize];

            var pointer = Marshal.AllocHGlobal(structureSize);

            Marshal.StructureToPtr(obj, pointer, true);

            Marshal.Copy(pointer, bytes, 0, structureSize);

            Marshal.FreeHGlobal(pointer);

            return bytes;
        }

        public static T Structure<T>(byte[] bytes)
            where T : struct
        {
            Ensure.ArgumentNotEmpty(bytes, nameof(bytes));

            var structureType = typeof(T);
            var structureSize = Marshal.SizeOf(structureType);

            if (bytes.Length != structureSize)
                throw new ArgumentOutOfRangeException(nameof(bytes), "Bytes array should be the same length with struct size.");

            var pointer = Marshal.AllocHGlobal(structureSize);

            Marshal.Copy(bytes, 0, pointer, structureSize);

            var structure = (T)Marshal.PtrToStructure(pointer, structureType);

            Marshal.FreeHGlobal(pointer);

            return structure;
        }
    }
}
