using System;
using System.Collections.Generic;
using System.Text;
using Gexf;
using System.Xml;

namespace NetLibrary
{
    static public class XmlGenerator
    {
        private const string SourceLabel = "Source";
        private const string LinkerLabel = "Linker";
        private const string TargetLabel = "Target";

        static private HashSet<Link> CollectLinks(Func<Link, bool> linkMatch)
        {
            var matchingLinks = new HashSet<Link>();

            Link.WalkThroughAllLinks(link =>
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
            var matchingLinks = new HashSet<Link>();

            Link.WalkThroughAllLinks(x => { matchingLinks.Add(x); });

            return matchingLinks;
        }

        static public string ToXml()
        {
            var matchingLinks = CollectLinks();

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                WriteXml(writer, matchingLinks);
            }
            return sb.ToString();
        }

        static public void ToFile(string path)
        {
            var matchingLinks = CollectLinks();

            Console.WriteLine("File write started.");

            using (var writer = XmlWriter.Create(path))
            {
                WriteXml(writer, matchingLinks);
            }

            Console.WriteLine("File write finished.");
        }

        static public void ToFile(string path, Func<Link, bool> filter)
        {
            var matchingLinks = CollectLinks(filter);

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
                    Graph.WriteXml(writer,
                        () => // nodes
                        {
                            foreach (Link matchingLink in matchingLinks)
                            {
                                Node.WriteXml(writer, matchingLink.ToInt(), matchingLink.ToString());
                            }
                        },
                        () => // edges
                        {
                            foreach (Link matchingLink in matchingLinks)
                            {
                                if (matchingLinks.Contains(matchingLink.Source))
                                {
                                    Edge.WriteXml(writer, edgesCounter++, matchingLink.ToInt(), matchingLink.Source.ToInt(), SourceLabel);
                                }
                                if (matchingLinks.Contains(matchingLink.Linker))
                                {
                                    Edge.WriteXml(writer, edgesCounter++, matchingLink.ToInt(), matchingLink.Linker.ToInt(), LinkerLabel);
                                }
                                if (matchingLinks.Contains(matchingLink.Target))
                                {
                                    Edge.WriteXml(writer, edgesCounter++, matchingLink.ToInt(), matchingLink.Target.ToInt(), TargetLabel);
                                }
                            }
                        }));
        }
    }
}
