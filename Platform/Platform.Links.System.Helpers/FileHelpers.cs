using System.IO;

namespace Platform.Links.System.Helpers
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

        public static FileStream Append(string path)
        {
            return File.Open(path, FileMode.Append, FileAccess.Write);
        }
    }
}
