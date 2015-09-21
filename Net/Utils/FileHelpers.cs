using System.IO;

namespace Utils
{
    public static class FileHelpers
    {
        public static char[] ReadAllChars(string path)
        {
            return File.ReadAllText(path).ToCharArray();
        }
    }
}
