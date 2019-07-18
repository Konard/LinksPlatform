using System;
using System.Runtime.InteropServices;
using Platform.Exceptions;

namespace Platform.Helpers.Unsafe
{
    public static class StructureExtensions
    {
        public static byte[] ToBytes<TStruct>(this TStruct obj)
            where TStruct : struct
        {
            var structureSize = StructureHelpers.SizeOf<TStruct>();

            var bytes = new byte[structureSize];

            var pointer = Marshal.AllocHGlobal(structureSize);

            Marshal.StructureToPtr(obj, pointer, true);

            Marshal.Copy(pointer, bytes, 0, structureSize);

            Marshal.FreeHGlobal(pointer);

            return bytes;
        }

        public static TTStruct ToStructure<TTStruct>(this byte[] bytes)
            where TTStruct : struct
        {
            Ensure.ArgumentNotEmpty(bytes, nameof(bytes));

            var structureSize = StructureHelpers.SizeOf<TTStruct>();

            if (bytes.Length != structureSize)
                throw new ArgumentOutOfRangeException(nameof(bytes), "Bytes array should be the same length as struct size.");

            var pointer = Marshal.AllocHGlobal(structureSize);

            Marshal.Copy(bytes, 0, pointer, structureSize);

            var structure = Marshal.PtrToStructure<TTStruct>(pointer);

            Marshal.FreeHGlobal(pointer);

            return structure;
        }
    }
}
