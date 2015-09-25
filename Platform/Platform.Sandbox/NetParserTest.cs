using System;

namespace Platform.Sandbox
{
    public class NetParserTest
    {
        public static void Run()
        {
            Console.WriteLine("Start typing into the net:");

            string line;
            while (!string.IsNullOrWhiteSpace(line = Console.ReadLine()))
            {
                NetParser.ParseStatement(line.Trim());
            };
        }
    }
}
