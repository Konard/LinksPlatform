using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Gexf
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
