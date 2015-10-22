using System;
using Platform.Data.Core.Pairs;
using Platform.Data.Core.Sequences;

namespace Platform.Sandbox
{
    public static class ConceptTest
    {
        public static void TestGexf(string filename)
        {
            using (var links = new Links.DataBase.CoreUnsafe.Pairs.Links(filename, 512*1024*1024))
            {
                const int linksToCreate = 1024;

                links.RunRandomCreations(linksToCreate);

                links.ExportSourcesTree(filename + ".gexf");

                Console.ReadKey();
            }
        }

        public static void Test(string filename)
        {
            //try
            {
                using (var links = new Links.DataBase.CoreUnsafe.Pairs.Links(filename, 512*1024*1024))
                {
                    //links.EnterTransaction();

                    links.TestBasicMemoryManagement();

                    Console.ReadKey();

                    ulong temp1 = links.Create(0, 0);
                    ulong temp2 = links.Create(0, 0);
                    ulong temp3 = links.Create(temp1, temp2);
                    ulong temp4 = links.Create(temp1, temp3);
                    ulong temp5 = links.Create(temp4, temp2);

                    //links.Delete(links.GetSource(temp2), links.GetTarget(temp2));

                    //links.Each(0, temp2, x => links.PrintLink(x));

                    links.Each(0, 0, x =>
                    {
                        links.PrintLink(x);
                        return true;
                    });

                    //links.ExportSourcesTree(filename + ".gexf");

                    Console.WriteLine("---");

                    Console.WriteLine(links.Total);

                    var sequences = new Sequences(links);

                    //var seq = sequences.Create(temp1, temp5, temp2, temp1, temp2); //, temp5);

                    ulong sequence = sequences.Create(temp1, temp5, temp2, temp1, temp2, temp3, temp2, temp4, temp1,
                        temp5);
                    //, temp5);

                    //links.Each(0, 0, (x, isAPoint) => { links.PrintLink(x); return true; });

                    //sequences.Each((x, isAPoint) => { links.PrintLink(x); return true; }, temp1, temp5, temp2, temp1, temp2, temp3, temp2, temp4, temp1, temp5);


                    int sequencesCount = 0;

                    sequences.Each(x =>
                    {
                        sequencesCount++;
                        return true;
                    }, temp1, temp5, temp2, temp1, temp2, temp3, temp2, temp4, temp1, temp5);

                    sequences.Compact(temp1, temp5, temp2, temp1, temp2, temp3, temp2, temp4, temp1, temp5);


                    Console.WriteLine(sequencesCount);

                    Console.WriteLine(links.Total);

                    sequences.Create(temp1, temp1, temp1, temp1, temp1, temp1, temp1, temp1, temp1, temp1, temp1, temp1,
                        temp1);

                    Console.WriteLine(links.Total);


                    Console.ReadKey();

                    //var ps = (from Pair score in links
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

                    links.Each(0, 0, x =>
                    {
                        links.PrintLink(x);
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

        private static void PrintLink(this Links.DataBase.CoreUnsafe.Pairs.Links links, ulong link)
        {
            Console.WriteLine(links.FormatLink(link));
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