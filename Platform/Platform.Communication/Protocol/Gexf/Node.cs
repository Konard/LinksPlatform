using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace Platform.Communication.Protocol.Gexf
{
    public class Node
    {
        public const string ElementName = "node";
        public const string IdAttributeName = "id";
        public const string LabelAttributeName = "label";

        [XmlAttribute(AttributeName = IdAttributeName)]
        public long Id { get; set; }

        [XmlAttribute(AttributeName = LabelAttributeName)]
        public string Label { get; set; }

        public void WriteXml(XmlWriter writer)
        {
            WriteXml(writer, Id, Label);
        }

        public static void WriteXml(XmlWriter writer, long id, string label)
        {
            // <node id="0" label="..." />
            writer.WriteStartElement(ElementName);

            writer.WriteAttributeString(IdAttributeName, id.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString(LabelAttributeName, label);

            writer.WriteEndElement();
        }
    }
}
