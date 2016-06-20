using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Platform.Communication.Protocol.Gexf;
using Platform.Helpers.Collections;

using GexfNode = Platform.Communication.Protocol.Gexf.Node;

namespace Platform.Data.Core.Triplets
{
    public static class GexfExporter
    {
        private const string SourceLabel = "Source";
        private const string LinkerLabel = "Linker";
        private const string TargetLabel = "Target";

        public static string ToXml()
        {
            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
                WriteXml(writer, CollectLinks());
            return sb.ToString();
        }

        public static void ToFile(string path)
        {
            using (var file = File.OpenWrite(path))
            using (var writer = XmlWriter.Create(file))
                WriteXml(writer, CollectLinks());
        }

        public static void ToFile(string path, Func<Link, bool> filter)
        {
            using (var file = File.OpenWrite(path))
            using (var writer = XmlWriter.Create(file))
                WriteXml(writer, CollectLinks(filter));
        }

        private static HashSet<Link> CollectLinks(Func<Link, bool> linkMatch)
        {
            var matchingLinks = new HashSet<Link>();
            Link.WalkThroughAllLinks(link =>
            {
                if (linkMatch(link)) matchingLinks.Add(link);
            });
            return matchingLinks;
        }

        private static HashSet<Link> CollectLinks()
        {
            var matchingLinks = new HashSet<Link>();
            Link.WalkThroughAllLinks((Action<Link>)matchingLinks.AddAndReturnVoid);
            return matchingLinks;
        }

        private static void WriteXml(XmlWriter writer, HashSet<Link> matchingLinks)
        {
            var edgesCounter = 0;

            Gexf.WriteXml(writer,
            () => // nodes
            {
                foreach (var matchingLink in matchingLinks)
                    GexfNode.WriteXml(writer, matchingLink.ToInt(), matchingLink.ToString());
            },
            () => // edges
            {
                foreach (var matchingLink in matchingLinks)
                {
                    if (matchingLinks.Contains(matchingLink.Source))
                        Edge.WriteXml(writer, edgesCounter++, matchingLink.ToInt(), matchingLink.Source.ToInt(), SourceLabel);
                    if (matchingLinks.Contains(matchingLink.Linker))
                        Edge.WriteXml(writer, edgesCounter++, matchingLink.ToInt(), matchingLink.Linker.ToInt(), LinkerLabel);
                    if (matchingLinks.Contains(matchingLink.Target))
                        Edge.WriteXml(writer, edgesCounter++, matchingLink.ToInt(), matchingLink.Target.ToInt(), TargetLabel);
                }
            });
        }
    }
}
