using System.Xml.Serialization;
using System.Xml;

namespace Gexf
{
    public class Edge
    {
        public const string ElementName = "edge";
        public const string IdAttributeName = "id";
        public const string SourceAttributeName = "source";
        public const string TargetAttributeName = "target";
        public const string LabelAttributeName = "label";

        [XmlAttribute(AttributeName = IdAttributeName)]
        public long Id { get; set; }

        [XmlAttribute(AttributeName = SourceAttributeName)]
        public long Source { get; set; }

        [XmlAttribute(AttributeName = TargetAttributeName)]
        public long Target { get; set; }

        [XmlAttribute(AttributeName = LabelAttributeName)]
        public string Label { get; set; }

        public void WriteXml(XmlWriter writer)
        {
            WriteXml(writer, this.Id, this.Source, this.Target, this.Label);
        }

        static public void WriteXml(XmlWriter writer, long id, long sourceNodeId, long targetNodeId, string label)
        {
            writer.WriteStartElement(ElementName);

            writer.WriteAttributeString(IdAttributeName, id.ToString());
            writer.WriteAttributeString(SourceAttributeName, sourceNodeId.ToString());
            writer.WriteAttributeString(TargetAttributeName, targetNodeId.ToString());
            writer.WriteAttributeString(LabelAttributeName, label);

            writer.WriteEndElement();
        }
    }
}
