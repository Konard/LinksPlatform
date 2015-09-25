using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace Platform.Communication.Protocol.Gexf
{
    public class Graph
    {
        public const string ElementName = "graph";
        public const string ModeAttributeName = "mode";
        public const string DefaultEdgeTypeAttributeName = "defaultedgetype";
        public const string NodesElementName = "nodes";
        public const string NodeElementName = "node";
        public const string EdgesElementName = "edges";
        public const string EdgeElementName = "edge";

        [XmlAttribute(AttributeName = ModeAttributeName)]
        public GraphMode Mode { get; set; }

        [XmlAttribute(AttributeName = DefaultEdgeTypeAttributeName)]
        public GraphDefaultEdgeType DefaultEdgeType { get; set; }

        [XmlArray(ElementName = NodesElementName)]
        [XmlArrayItem(ElementName = NodeElementName)]
        public List<Node> Nodes { get; set; }

        [XmlArray(ElementName = EdgesElementName)]
        [XmlArrayItem(ElementName = EdgeElementName)]
        public List<Edge> Edges { get; set; }

        public Graph()
        {
            Nodes = new List<Node>();
            Edges = new List<Edge>();
        }

        public void WriteXml(XmlWriter writer)
        {
            Action writeNodes = () =>
                {
                    for (int i = 0; i < Nodes.Count; i++)
                    {
                        Nodes[i].WriteXml(writer);
                    }
                };

            Action writeEdges = () =>
                {
                    for (int i = 0; i < Edges.Count; i++)
                    {
                        Edges[i].WriteXml(writer);
                    }
                };

            WriteXml(writer, writeNodes, writeEdges, Mode, DefaultEdgeType);
        }

        public static void WriteXml(XmlWriter writer, Action writeNodes, Action writeEdges, GraphMode mode = GraphMode.Static, GraphDefaultEdgeType defaultEdgeType = GraphDefaultEdgeType.Directed)
        {
            writer.WriteStartElement(ElementName);

            writer.WriteAttributeString(ModeAttributeName, mode.ToString().ToLower());
            writer.WriteAttributeString(DefaultEdgeTypeAttributeName, defaultEdgeType.ToString().ToLower());

            writer.WriteStartElement(NodesElementName);

            writeNodes();

            writer.WriteEndElement();

            writer.WriteStartElement(EdgesElementName);

            writeEdges();

            writer.WriteEndElement();

            writer.WriteEndElement();
        }
    }
}
