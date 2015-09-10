using System.Runtime.InteropServices;

namespace Platform.Links.System.Helpers
{
    public static class BitConverterExtensions
    {
        public static byte[] GetBytes<T>(T obj)
            where T : struct
        {
            var len = Marshal.SizeOf(obj);

            var arr = new byte[len];

            var ptr = Marshal.AllocHGlobal(len);

            Marshal.StructureToPtr(obj, ptr, true);

            Marshal.Copy(ptr, arr, 0, len);

            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        public static T ToStructure<T>(byte[] bytes)
            where T : struct
        {
            var structureType = typeof (T);

            var len = Marshal.SizeOf(structureType);

            var i = Marshal.AllocHGlobal(len);

            Marshal.Copy(bytes, 0, i, len);

            var result = (T) Marshal.PtrToStructure(i, structureType);

            Marshal.FreeHGlobal(i);

            return result;
        }
    }
}