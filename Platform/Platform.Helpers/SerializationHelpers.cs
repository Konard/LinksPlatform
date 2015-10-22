using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Platform.Helpers
{
    static public class SerializationHelpers
    {
        static readonly Dictionary<Type, XmlSerializer> XmlSerializerCache = new Dictionary<Type, XmlSerializer>();

        static public T DeserializeFromXml<T>(string xmlString)
        {
            var serializer = GetXmlSerializer<T>();
            using (var reader = new StringReader(xmlString))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        static public void SerializeToFile<T>(string path, T obj)
        {
            var serializer = GetXmlSerializer<T>();
            using (var fileStream = File.Open(path, FileMode.Create))
            {
                serializer.Serialize(fileStream, obj);
                fileStream.Flush();
            }
        }

        static public string SerializeAsXmlString<T>(T obj)
        {
            var serializer = GetXmlSerializer<T>();
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, obj);
                writer.Flush();
                return sb.ToString();
            }
        }

        static XmlSerializer GetXmlSerializer<T>()
        {
            XmlSerializer serializer;
            var type = typeof(T);
            if (!XmlSerializerCache.TryGetValue(type, out serializer))
            {
                serializer = new XmlSerializer(type);
                XmlSerializerCache.Add(type, serializer);
            }
            return serializer;
        }
    }
}
