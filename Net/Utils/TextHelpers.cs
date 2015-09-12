namespace Utils
{
    public class TextHelpers
    {
        static public string CapitalizeFirstLetter(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
            return str.Substring(0, 1).ToUpper() + str.Substring(1, str.Length - 1);
        }
    }
}