using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Platform.Helpers
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
            if (File.Exists(path))
                using (var reader = File.OpenRead(path))
                    return reader.ReadOrDefault<T>();

            return default(T);
        }

        public static T ReadLastOrDefault<T>(string path)
            where T : struct
        {
            if (File.Exists(path))
                using (var reader = File.OpenRead(path))
                {
                    var elementSize = Marshal.SizeOf<T>();

                    if (reader.Length % elementSize != 0)
                        throw new NotSupportedException(string.Format("File is not aligned to elements with size {0}.", elementSize));

                    if (reader.Length < elementSize)
                        reader.Position = 0;
                    else
                    {
                        var totalElements = reader.Length / elementSize;
                        reader.Position = (totalElements - 1) * elementSize; // Set to last element
                    }

                    return reader.ReadOrDefault<T>();
                }

            return default(T);
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

        public static FileStream Append(string path)
        {
            return File.Open(path, FileMode.Append, FileAccess.Write);
        }
    }
}
