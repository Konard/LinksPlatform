using System;
using System.Xml.Serialization;
using System.Xml;

namespace Gexf
{
    [Serializable]
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
            this.Version = CurrentVersion;
            this.Graph = new Graph();
        }

        public void WriteXml(XmlWriter writer)
        {
            Action writeGraph = () =>
            {
                this.Graph.WriteXml(writer);
            };

            WriteXml(writer, writeGraph, this.Version);
        }

        static public void WriteXml(XmlWriter writer, Action writeGraph, string version = Gexf.CurrentVersion)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement(ElementName, Namespace);
            writer.WriteAttributeString(VersionAttributeName, version);

            writeGraph();

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }
    }
}
