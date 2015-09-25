using System.Xml.Serialization;

namespace Platform.Communication.Protocol.Gexf
{
    public enum GraphMode
    {
        [XmlEnum(Name = "static")]
        Static,

        [XmlEnum(Name = "dynamic")]
        Dynamic
    }

    public enum GraphDefaultEdgeType
    {
        [XmlEnum(Name = "directed")]
        Directed
    }
}
