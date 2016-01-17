using System;
using System.Text;

namespace Platform.Helpers
{
    public static class ExceptionExtensions
    {
        public const string ExceptionContentsSeparator = "---";
        public const string ExceptionFailed = "Unable to format occured exception.";

        public static string ToRecursiveString(this Exception ex)
        {
            try
            {
                var sb = new StringBuilder();
                ToRecursiveStringCore(sb, ex, 0);
                return sb.ToString();
            }
            catch
            {
                return ExceptionFailed;
            }
        }

        private static void ToRecursiveStringCore(StringBuilder sb, Exception ex, int level)
        {
            sb.Append('\t', level);
            sb.Append("Exception message: ");
            sb.AppendLine(ex.Message);

            sb.Append('\t', level);
            sb.AppendLine(ExceptionContentsSeparator);

            if (ex.InnerException != null)
            {
                sb.Append('\t', level);
                sb.AppendLine("Inner Exception: ");

                ToRecursiveStringCore(sb, ex.InnerException, level + 1);
            }

            sb.Append('\t', level);
            sb.AppendLine(ExceptionContentsSeparator);

            sb.Append('\t', level);
            sb.AppendLine(ex.StackTrace);
        }
    }
}
