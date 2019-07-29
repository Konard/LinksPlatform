using System;
using System.Collections.Generic;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.ResizableDirectMemory;
using Platform.Data.Doublets.Sequences;

namespace Platform.Sandbox
{
    public static class ConceptTest
    {
        public static void TestGexf(string filename)
        {
            using (var memoryManager = new UInt64ResizableDirectMemoryLinks(filename, 512 * 1024 * 1024))
            using (var links = new UInt64Links(memoryManager))
            {
                //var options = new LinksOptions<ulong>();
                //options.MemoryManager = memoryManager;
                //var linksFactory = new LinksFactory<ulong>(options);
                //var links = linksFactory.Create();

                const int linksToCreate = 1024;

                links.RunRandomCreations(linksToCreate);

                //memoryManager.ExportSourcesTree(filename + ".gexf");

                Console.ReadKey();
            }
        }

        public static void Test(string filename)
        {
            //try
            {
                using (var memoryManager = new UInt64ResizableDirectMemoryLinks(filename, 512 * 1024 * 1024))
                using (var links = new UInt64Links(memoryManager))
                {
                    var syncLinks = new SynchronizedLinks<ulong>(links);
                    //links.EnterTransaction();

                    var link = memoryManager.Create();
                    memoryManager.Delete(link);

                    Console.ReadKey();

                    var temp1 = syncLinks.Create();
                    var temp2 = syncLinks.Create();
                    var temp3 = syncLinks.CreateAndUpdate(temp1, temp2);
                    var temp4 = syncLinks.CreateAndUpdate(temp1, temp3);
                    var temp5 = syncLinks.CreateAndUpdate(temp4, temp2);

                    //links.Delete(links.GetSource(temp2), links.GetTarget(temp2));

                    //links.Each(0, temp2, x => links.PrintLink(x));

                    syncLinks.Each(syncLinks.Constants.Any, syncLinks.Constants.Any, x =>
                    {
                        memoryManager.PrintLink(x);
                        return true;
                    });

                    //links.ExportSourcesTree(filename + ".gexf");

                    Console.WriteLine("---");

                    Console.WriteLine(syncLinks.Count());

                    var sequences = new Sequences(syncLinks);

                    //var seq = sequences.Create(temp1, temp5, temp2, temp1, temp2); //, temp5);

                    var sequence = sequences.Create(temp1, temp5, temp2, temp1, temp2, temp3, temp2, temp4, temp1,
                        temp5);
                    //, temp5);

                    //links.Each(0, 0, (x, isAPoint) => { links.PrintLink(x); return true; });

                    //sequences.Each((x, isAPoint) => { links.PrintLink(x); return true; }, temp1, temp5, temp2, temp1, temp2, temp3, temp2, temp4, temp1, temp5);


                    var sequencesCount = 0;

                    //sequences.Each(x =>
                    //{
                    //    sequencesCount++;
                    //    return true;
                    //}, temp1, temp5, temp2, temp1, temp2, temp3, temp2, temp4, temp1, temp5);

                    sequences.Compact(temp1, temp5, temp2, temp1, temp2, temp3, temp2, temp4, temp1, temp5);


                    Console.WriteLine(sequencesCount);

                    Console.WriteLine(syncLinks.Count());

                    sequences.Create(temp1, temp1, temp1, temp1, temp1, temp1, temp1, temp1, temp1, temp1, temp1, temp1,
                        temp1);

                    Console.WriteLine(syncLinks.Count());


                    Console.ReadKey();

                    //var ps = (from Doublet score in links
                    //          where score.Target == temp2
                    //          select score).ToArray();

                    //var ls = (from Link score in links
                    //          where score.Target == temp2
                    //          select score).ToArray();

                    //links.Execute(db => from User user in links
                    //                    select user);

                    //var firstLink = links.First();

                    //links.Delete(ref firstLink);

                    Console.WriteLine("---");

                    syncLinks.Each(syncLinks.Constants.Any, syncLinks.Constants.Any, x =>
                    {
                        memoryManager.PrintLink(x);
                        return true;
                    });

                    Console.WriteLine("---");

                    //links.ExitTransaction();

                    //links.EnterTransaction();

                    //links.ExitTransaction();
                }
                ;
            }
            //catch (Exception ex)
            {
                //    ex.WriteToConsole();
            }

            Console.ReadKey();
        }

        private static void PrintLink(this UInt64ResizableDirectMemoryLinks links, ulong link)
        {
            Console.WriteLine(links.FormatLink(links.GetLink(link)));
        }

        private static string FormatLink(this UInt64ResizableDirectMemoryLinks links, IList<ulong> link)
        {
            const string format = "{1} {0} {2}"; // "{0}: {1} -> {2}"

            return string.Format(format, link[links.Constants.IndexPart], link[links.Constants.SourcePart], link[links.Constants.TargetPart]);
        }

        private class User
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Username { get; set; }
        }

        private class UserGroup
        {
            public long Id { get; set; }
            public string[] AllowedLocations { get; set; }
            public string[] DisallowedLocations { get; set; }
        }

        private class UserRight
        {
            public long Id { get; set; }
            public long UserGroupsIds { get; set; }
        }
    }
}