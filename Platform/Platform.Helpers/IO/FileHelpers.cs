using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Platform.Helpers.IO
{
    public static class FileHelpers
    {
        public static char[] ReadAllChars(string path)
        {
            return File.ReadAllText(path).ToCharArray();
        }

        public static T[] ReadAll<T>(string path)
            where T : struct
        {
            using (var reader = File.OpenRead(path))
                return reader.ReadAll<T>();
        }

        public static T ReadFirstOrDefault<T>(string path)
            where T : struct
        {
            using (var fileStream = GetValidFileStreamOrDefault<T>(path))
                return fileStream?.ReadOrDefault<T>() ?? default(T);
        }

        private static FileStream GetValidFileStreamOrDefault<TStruct>(string path)
            where TStruct : struct
        {
            var elementSize = Marshal.SizeOf<TStruct>();
            return GetValidFileStreamOrDefault(path, elementSize);
        }

        private static FileStream GetValidFileStreamOrDefault(string path, int elementSize)
        {
            if (!File.Exists(path))
                return null;

            var fileSize = GetSize(path);

            if (fileSize % elementSize != 0)
                throw new NotSupportedException($"File is not aligned to elements with size {elementSize}.");

            return fileSize != 0 ? File.OpenRead(path) : null;
        }

        public static T ReadLastOrDefault<T>(string path)
            where T : struct
        {
            var elementSize = Marshal.SizeOf<T>();
            using (var reader = GetValidFileStreamOrDefault(path, elementSize))
            {
                if (reader == null)
                    return default(T);

                var totalElements = reader.Length / elementSize;
                reader.Position = (totalElements - 1) * elementSize; // Set to last element

                return reader.ReadOrDefault<T>();
            }
        }

        public static void WriteFirst<T>(string path, T value)
            where T : struct
        {
            using (var writer = File.OpenWrite(path))
            {
                writer.Position = 0;
                writer.Write(value);
            }
        }

        public static FileStream Append(string path) => File.Open(path, FileMode.Append, FileAccess.Write);

        public static long GetSize(string path) => File.Exists(path) ? new FileInfo(path).Length : 0;

        public static void SetSize(string path, long size)
        {
            using (var fileStream = File.Open(path, FileMode.OpenOrCreate))
                if (fileStream.Length != size)
                    fileStream.SetLength(size);
        }
    }
}
