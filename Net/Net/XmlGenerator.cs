using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gexf;
using System.Threading;
using System.Xml;
using System.Threading.Tasks;

namespace NetLibrary
{
    static public class XmlGenerator
    {
        static private readonly string SourceLabel = "Source";
        static private readonly string LinkerLabel = "Linker";
        static private readonly string TargetLabel = "Target";

        static private HashSet<Link> CollectLinks(Func<Link, bool> linkMatch)
        {
            HashSet<Link> matchingLinks = new HashSet<Link>();

            Link.WalkThroughAllLinks((link) =>
                {
                    if (linkMatch(link))
                    {
                        matchingLinks.Add(link);
                    }
                });

            return matchingLinks;
        }

        static private HashSet<Link> CollectLinks()
        {
            HashSet<Link> matchingLinks = new HashSet<Link>();

            Link.WalkThroughAllLinks(x => { matchingLinks.Add(x); });

            return matchingLinks;
        }

        static public string ToXml()
        {
            HashSet<Link> matchingLinks = CollectLinks();

            StringBuilder sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                WriteXml(writer, matchingLinks);
            }
            return sb.ToString();
        }

        static public void ToFile(string path)
        {
            HashSet<Link> matchingLinks = CollectLinks();

            Console.WriteLine("File write started.");

            using (var writer = XmlWriter.Create(path))
            {
                WriteXml(writer, matchingLinks);
            }

            Console.WriteLine("File write finished.");
        }

        static public void ToFile(string path, Func<Link, bool> filter)
        {
            HashSet<Link> matchingLinks = CollectLinks(filter);

            Console.WriteLine("File write started.");

            using (var writer = XmlWriter.Create(path))
            {
                WriteXml(writer, matchingLinks);
            }

            Console.WriteLine("File write finished");
        }

        static private void WriteXml(XmlWriter writer, HashSet<Link> matchingLinks)
        {
            int edgesCounter = 0;

            Gexf.Gexf.WriteXml(writer,
                () => // graph
                {
                    Graph.WriteXml(writer,
                        () => // nodes
                        {
                            foreach (Link matchingLink in matchingLinks)
                            {
								Node.WriteXml(writer, matchingLink.GetPointer().ToInt64(), matchingLink.ToString());
                            }
                        },
                        () => // edges
                        {
                            foreach (Link matchingLink in matchingLinks)
                            {
                                if (matchingLinks.Contains(matchingLink.Source))
                                {
									Edge.WriteXml(writer, edgesCounter++, matchingLink.GetPointer().ToInt64(), matchingLink.Source.GetPointer().ToInt64(), SourceLabel);
                                }
                                if (matchingLinks.Contains(matchingLink.Linker))
                                {
									Edge.WriteXml(writer, edgesCounter++, matchingLink.GetPointer().ToInt64(), matchingLink.Linker.GetPointer().ToInt64(), LinkerLabel);
                                }
                                if (matchingLinks.Contains(matchingLink.Target))
                                {
									Edge.WriteXml(writer, edgesCounter++, matchingLink.GetPointer().ToInt64(), matchingLink.Target.GetPointer().ToInt64(), TargetLabel);
                                }
                            }
                        });
                });
        }
    }
}
