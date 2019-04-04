using System;
using System.Xml;
using System.Xml.Serialization;

namespace Platform.Communication.Protocol.Gexf
{
    [XmlRoot(ElementName = ElementName, Namespace = Namespace)]
    public class Gexf
    {
        public const string ElementName = "gexf";
        public const string Namespace = "http://www.gexf.net/1.2draft";
        public const string VersionAttributeName = "version";
        public const string GraphElementName = "graph";
        public const string CurrentVersion = "1.2";

        [XmlAttribute(AttributeName = VersionAttributeName)]
        public string Version { get; set; }

        [XmlElement(ElementName = GraphElementName)]
        public Graph Graph { get; set; }

        public Gexf()
        {
            Version = CurrentVersion;
            Graph = new Graph();
        }

        public void WriteXml(XmlWriter writer)
        {
            Action writeGraph = () => Graph.WriteXml(writer);

            WriteXml(writer, writeGraph, Version);
        }

        public static void WriteXml(XmlWriter writer, Action writeGraph, string version = CurrentVersion)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement(ElementName, Namespace);
            writer.WriteAttributeString(VersionAttributeName, version);

            writeGraph();

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        public static void WriteXml(XmlWriter writer, Action writeNodes, Action writeEdges, string version = CurrentVersion,
            GraphMode mode = GraphMode.Static, GraphDefaultEdgeType defaultEdgeType = GraphDefaultEdgeType.Directed)
        {
            WriteXml(writer, () => Graph.WriteXml(writer, writeNodes, writeEdges, mode, defaultEdgeType), version);
        }
    }
}
