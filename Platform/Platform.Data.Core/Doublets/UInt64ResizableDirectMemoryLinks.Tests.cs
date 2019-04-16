using System.IO;
using System.Xml;
using Platform.Communication.Protocol.Gexf;
using Platform.Data.Core.Exceptions;
using Platform.Helpers;
using GexfNode = Platform.Communication.Protocol.Gexf.Node;

namespace Platform.Data.Core.Doublets
{
    /// <summary>
    /// TODO: Make separate GEXF Exporter.
    /// </summary>
    unsafe partial class UInt64ResizableDirectMemoryLinks
    {
        public void ExportSourcesTree(string outputFilename)
        {
            using (var file = File.OpenWrite(outputFilename))
            using (var writer = XmlWriter.Create(file))
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
                        GexfNode.WriteXml(writer, (long)link, FormatLink(link));

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
                            Edge.WriteXml(writer, (long)edges++, (long)link, (long)_links[link].LeftAsSource);

                        if (_links[link].RightAsSource != 0)
                            Edge.WriteXml(writer, (long)edges++, (long)link, (long)_links[link].RightAsSource);
                    }
                }
                // </edges>
                writer.WriteEndElement();

                // </graph>
                writer.WriteEndElement();

                // </gexf>
                writer.WriteEndElement();

                writer.WriteEndDocument();

                ConsoleHelpers.Debug("Head of Sources: {0}", _header->FirstAsSource);
            }
        }

        public void ExportTargetsTree(string outputFilename)
        {
            using (var file = File.OpenWrite(outputFilename))
            using (var writer = XmlWriter.Create(file))
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
                        GexfNode.WriteXml(writer, (long)link, FormatLink(link));

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
                            Edge.WriteXml(writer, (long)edges++, (long)link, (long)_links[link].LeftAsTarget);

                        if (_links[link].RightAsTarget != 0)
                            Edge.WriteXml(writer, (long)edges++, (long)link, (long)_links[link].RightAsTarget);
                    }
                }
                // </edges>
                writer.WriteEndElement();

                // </graph>
                writer.WriteEndElement();

                // </gexf>
                writer.WriteEndElement();

                writer.WriteEndDocument();

                ConsoleHelpers.Debug("Head of Targets: {0}", _header->FirstAsTarget);
            }
        }

        public void Export(string outputFilename)
        {
            using (var file = File.OpenWrite(outputFilename))
            using (var writer = XmlWriter.Create(file))
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
                        GexfNode.WriteXml(writer, (long)link, FormatLink(link));

                // </nodes>
                writer.WriteEndElement();

                ulong edges = 0;

                // <edges>
                writer.WriteStartElement("edges");
                for (ulong link = 1; link <= _header->AllocatedLinks; link++)
                    if (Exists(link))
                        Edge.WriteXml(writer, (long)edges++, (long)_links[link].Source, (long)_links[link].Target);

                // </edges>
                writer.WriteEndElement();

                // </graph>
                writer.WriteEndElement();

                // </gexf>
                writer.WriteEndElement();

                writer.WriteEndDocument();

                ConsoleHelpers.Debug("Head of Sources: {0}", _header->FirstAsSource);
            }
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