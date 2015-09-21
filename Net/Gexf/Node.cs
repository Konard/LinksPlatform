using System.Xml.Serialization;
using System.Xml;

namespace Gexf
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

        static public void WriteXml(XmlWriter writer, long id, string label)
        {
            writer.WriteStartElement(ElementName);

            writer.WriteAttributeString(IdAttributeName, id.ToString());
            writer.WriteAttributeString(LabelAttributeName, label);

            writer.WriteEndElement();
        }
    }
}
