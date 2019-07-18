using System.IO;
using Platform.Helpers.Unsafe;

namespace Platform.Helpers.IO
{
    public static class StreamExtensions
    {
        public static void Write<T>(this Stream stream, T value)
            where T : struct
        {
            var bytes = value.ToBytes();
            stream.Write(bytes, 0, bytes.Length);
        }

        public static T ReadOrDefault<T>(this Stream stream)
            where T : struct
        {
            var size = StructureHelpers.SizeOf<T>();
            var buffer = new byte[size];
            var read = stream.Read(buffer, 0, size);
            return read < size ? default : buffer.ToStructure<T>();
        }

        public static T[] ReadAll<T>(this Stream stream)
            where T : struct
        {
            var size = StructureHelpers.SizeOf<T>();
            var buffer = new byte[size];
            var elementsLength = stream.Length / size;
            var elements = new T[elementsLength];

            for (var i = 0; i < elementsLength; i++)
            {
                stream.Read(buffer, 0, size);
                elements[i] = buffer.ToStructure<T>();
            }

            return elements;
        }
    }
}
