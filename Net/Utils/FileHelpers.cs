using System.IO;

namespace Utils
{
    static public class FileHelpers
    {
        static public char[] ReadAllChars(string path)
        {
            return File.ReadAllText(path).ToCharArray();
        }
    }
}
