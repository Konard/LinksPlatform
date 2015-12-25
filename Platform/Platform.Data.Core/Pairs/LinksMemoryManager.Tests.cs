using System;
using System.Globalization;
using System.Xml;
using Platform.Data.Core.Exceptions;

namespace Platform.Data.Core.Pairs
{
    unsafe partial class LinksMemoryManager
    {
        public void ExportSourcesTree(string outputFilename)
        {
            using (var writer = XmlWriter.Create(outputFilename))
            {
                // <?xml version="1.0" encoding="UTF-8"?>
                writer.WriteStartDocument();

                // <gexf xmlns="http://www.gexf.net/1.2draft" version="1.2">
                writer.WriteStartElement("gexf", "http://www.gexf.net/1.2draft");
                writer.WriteAttributeString("version", "1.2");

                // <graph mode="static" defaultedgetype="directed">
                writer.WriteStartElement("graph");
                writer.WriteAttributeString("mode", "static");
                writer.WriteAttributeString("defaultedgetype", "directed");

                // <nodes>
                writer.WriteStartElement("nodes");
                for (ulong link = 1; link <= _header->AllocatedLinks; link++)
                    if (Exists(link))
                        WriteNode(writer, link, FormatLink(link));

                // </nodes>
                writer.WriteEndElement();

                ulong edges = 0;

                // <edges>
                writer.WriteStartElement("edges");
                for (ulong link = 1; link <= _header->AllocatedLinks; link++)
                {
                    if (Exists(link))
                    {
                        if (_links[link].LeftAsSource != 0)
                            WriteEdge(writer, edges++, link, _links[link].LeftAsSource);

                        if (_links[link].RightAsSource != 0)
                            WriteEdge(writer, edges++, link, _links[link].RightAsSource);
                    }
                }
                // </edges>
                writer.WriteEndElement();

                // </graph>
                writer.WriteEndElement();

                // </gexf>
                writer.WriteEndElement();

                writer.WriteEndDocument();

                Console.WriteLine("Head of Sources: {0}", _header->FirstAsSource);
            }
        }

        public void ExportTargetsTree(string outputFilename)
        {
            using (var writer = XmlWriter.Create(outputFilename))
            {
                // <?xml version="1.0" encoding="UTF-8"?>
                writer.WriteStartDocument();

                // <gexf xmlns="http://www.gexf.net/1.2draft" version="1.2">
                writer.WriteStartElement("gexf", "http://www.gexf.net/1.2draft");
                writer.WriteAttributeString("version", "1.2");

                // <graph mode="static" defaultedgetype="directed">
                writer.WriteStartElement("graph");
                writer.WriteAttributeString("mode", "static");
                writer.WriteAttributeString("defaultedgetype", "directed");

                // <nodes>
                writer.WriteStartElement("nodes");
                for (ulong link = 1; link <= _header->AllocatedLinks; link++)
                    if (Exists(link))
                        WriteNode(writer, link, FormatLink(link));

                // </nodes>
                writer.WriteEndElement();

                ulong edges = 0;

                // <edges>
                writer.WriteStartElement("edges");
                for (ulong link = 1; link <= _header->AllocatedLinks; link++)
                {
                    if (Exists(link))
                    {
                        if (_links[link].LeftAsTarget != 0)
                            WriteEdge(writer, edges++, link, _links[link].LeftAsTarget);

                        if (_links[link].RightAsTarget != 0)
                            WriteEdge(writer, edges++, link, _links[link].RightAsTarget);
                    }
                }
                // </edges>
                writer.WriteEndElement();

                // </graph>
                writer.WriteEndElement();

                // </gexf>
                writer.WriteEndElement();

                writer.WriteEndDocument();

                Console.WriteLine("Head of Targets: {0}", _header->FirstAsTarget);
            }
        }

        public void Export(string outputFilename)
        {
            using (var writer = XmlWriter.Create(outputFilename))
            {
                // <?xml version="1.0" encoding="UTF-8"?>
                writer.WriteStartDocument();

                // <gexf xmlns="http://www.gexf.net/1.2draft" version="1.2">
                writer.WriteStartElement("gexf", "http://www.gexf.net/1.2draft");
                writer.WriteAttributeString("version", "1.2");

                // <graph mode="static" defaultedgetype="directed">
                writer.WriteStartElement("graph");
                writer.WriteAttributeString("mode", "static");
                writer.WriteAttributeString("defaultedgetype", "directed");

                // <nodes>
                writer.WriteStartElement("nodes");
                for (ulong link = 1; link <= _header->AllocatedLinks; link++)
                    if (Exists(link))
                        WriteNode(writer, link, FormatLink(link));

                // </nodes>
                writer.WriteEndElement();

                ulong edges = 0;

                // <edges>
                writer.WriteStartElement("edges");
                for (ulong link = 1; link <= _header->AllocatedLinks; link++)
                    if (Exists(link))
                        WriteEdge(writer, edges++, _links[link].Source, _links[link].Target);

                // </edges>
                writer.WriteEndElement();

                // </graph>
                writer.WriteEndElement();

                // </gexf>
                writer.WriteEndElement();

                writer.WriteEndDocument();

                Console.WriteLine("Head of Sources: {0}", _header->FirstAsSource);
            }
        }

        private static void WriteNode(XmlWriter writer, ulong link, string label)
        {
            // <node id="0" label="0" />
            writer.WriteStartElement("node");
            writer.WriteAttributeString("id", link.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("label", label);
            writer.WriteEndElement();
        }

        private static void WriteEdge(XmlWriter writer, ulong id, ulong source, ulong target)
        {
            // <edge id="0" source="0" target="1" />
            writer.WriteStartElement("edge");
            writer.WriteAttributeString("id", id.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("source", source.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("target", target.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
        }

        public string FormatLink(ulong link)
        {
            //const string format = "{0}: {1} -> {2}";
            const string format = "{1} {0} {2}";

            if (!Exists(link))
                throw new ArgumentLinkDoesNotExistsException<ulong>(link, "link");

            return string.Format(format, link, _links[link].Source, _links[link].Target);
        }
    }
}