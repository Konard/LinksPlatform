using System;
using System.Runtime.InteropServices;

namespace Platform.Helpers
{
    public static class BitConverterHelpers
    {
        public static byte[] GetBytes<T>(T obj)
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

        public static T ToStructure<T>(byte[] bytes)
            where T : struct
        {
            if (bytes == null || bytes.Length == 0)
                throw new ArgumentNullException("bytes");

            var structureType = typeof (T);

            var structureSize = Marshal.SizeOf(structureType);

            if (bytes.Length != structureSize)
                throw new ArgumentOutOfRangeException("bytes.Length", "Bytes array should be the same length with struct size.");

            var pointer = Marshal.AllocHGlobal(structureSize);

            Marshal.Copy(bytes, 0, pointer, structureSize);

            var structure = (T) Marshal.PtrToStructure(pointer, structureType);

            Marshal.FreeHGlobal(pointer);

            return structure;
        }
    }
}