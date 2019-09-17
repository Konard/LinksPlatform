using System;
using System.Text.RegularExpressions;

namespace Platform.Sandbox
{
    /// <remarks>
    /// TODO: Use PEG instead.
    /// </remarks>
    public class NetParser
    {
        const string StatementTermPart = @"((?<term>\w+)|""(?<termstring>[^""]+)""):";
        const string StatementPart = @"(?<part>((?<word>\w+)|""(?<string>[^""]+)""))";
        const string Statement = "^(" + StatementTermPart + "|" + StatementPart + @")(\s+" + StatementPart + @")+\.$";

        private static readonly Regex _statementRegex = new Regex(Statement, RegexOptions.Compiled);

        public static void ParseStatement(string statement)
        {
            var match = _statementRegex.Match(statement);
            if (match.Success)
            {
                Console.WriteLine("-> Statement parsed successfully.");

                foreach (Capture capture in match.Groups["part"].Captures)
                {
                    var value = capture.Value.ToLower();

                    throw new NotImplementedException();
                }
            }

            //if (statement.Length < MinStatementLength)
            //{
            //    Console.WriteLine("Утверждение не может содержать меньше {0} символов.", MinStatementLength);
            //}
            //if (!char.IsUpper(statement[0]))
            //{
            //    Console.WriteLine("Утверждение должно начинаться с заглавной буквы.");
            //}
            //if(!char.
        }
    }
}
