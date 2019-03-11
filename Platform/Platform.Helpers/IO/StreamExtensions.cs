using System.IO;
using System.Runtime.InteropServices;

namespace Platform.Helpers.IO
{
    public static class StreamExtensions
    {
        public static void Write<T>(this Stream stream, T value)
            where T : struct
        {
            var bytes = To.Bytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static T ReadOrDefault<T>(this Stream stream)
            where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var buffer = new byte[size];
            var read = stream.Read(buffer, 0, size);
            return read < size ? default(T) : To.Structure<T>(buffer);
        }

        public static T[] ReadAll<T>(this Stream stream)
            where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var buffer = new byte[size];
            var elementsLength = stream.Length / size;
            var elements = new T[elementsLength];

            for (var i = 0; i < elementsLength; i++)
            {
                stream.Read(buffer, 0, size);
                elements[i] = To.Structure<T>(buffer);
            }

            return elements;
        }
    }
}
