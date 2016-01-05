using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace Platform.Communication.Protocol.Gexf
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
            WriteXml(writer, Id, Source, Target, Label);
        }

        public static void WriteXml(XmlWriter writer, long id, long sourceNodeId, long targetNodeId, string label = null)
        {
            // <edge id="0" source="0" target="0" label="..." />
            writer.WriteStartElement(ElementName);

            writer.WriteAttributeString(IdAttributeName, id.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(SourceAttributeName, sourceNodeId.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(TargetAttributeName, targetNodeId.ToString(CultureInfo.InvariantCulture));
            if (!string.IsNullOrWhiteSpace(label))
                writer.WriteAttributeString(LabelAttributeName, label);

            writer.WriteEndElement();
        }
    }
}
