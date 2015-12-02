using System;
using Platform.Data.Core.Triplets;

namespace Platform.Sandbox
{
    public static class TerminalExperiment
    {
        public static bool IsRunning { get; set; }

        public static void Run()
        {
            Link.CreatedEvent += LinkCreated;

            Link x = Net.CreateThing();

            Link number = NumberHelpers.FromNumber(4637694687);


            return;

            do
            {
                Console.Write("→ ");
                string readMessage = Console.ReadLine();

                if (string.Compare("exit", readMessage, ignoreCase: true) == 0)
                    break;

                Console.Write("← ");
                Console.WriteLine(readMessage);
            }
            while (true);
        }

        public static void LinkCreated(LinkDefinition createdLink)
        {
            Console.WriteLine("Link created: {0} {1} {2}", createdLink.Source.ToIndex(), createdLink.Linker.ToIndex(), createdLink.Target.ToIndex());
        }
    }
}
