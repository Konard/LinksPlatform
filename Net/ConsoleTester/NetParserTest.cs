using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleTester
{
    public class NetParserTest
    {
        static public void Run()
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
