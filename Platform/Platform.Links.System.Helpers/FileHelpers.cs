using System.IO;

namespace Platform.Links.System.Helpers
{
    public static class FileHelpers
    {
        public static char[] ReadAllChars(string path)
        {
            return File.ReadAllText(path).ToCharArray();
        }
    }
}
