using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace Gexf
{
    static public class SerializationHelpers
    {
        static readonly Dictionary<Type, XmlSerializer> XmlSerializerCache = new Dictionary<Type, XmlSerializer>();

        static public T DeserializeFromXml<T>(string xmlString)
        {
            XmlSerializer serializer = GetXmlSerializer<T>();
            using (StringReader reader = new StringReader(xmlString))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        static public void SerializeToFile<T>(string path, T obj)
        {
            XmlSerializer serializer = GetXmlSerializer<T>();
            using (var fileStream = File.Open(path, FileMode.Create))
            {
                serializer.Serialize(fileStream, obj);
                fileStream.Flush();
            }
        }

        static public string SerializeAsXmlString<T>(T obj)
        {
            XmlSerializer serializer = GetXmlSerializer<T>();
            StringBuilder sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, obj);
                writer.Flush();
                return sb.ToString();
            }
        }

        static XmlSerializer GetXmlSerializer<T>()
        {
            XmlSerializer serializer;
            Type type = typeof(T);
            if (!XmlSerializerCache.TryGetValue(type, out serializer))
            {
                serializer = new XmlSerializer(type);
                XmlSerializerCache.Add(type, serializer);
            }
            return serializer;
        }
    }
}
