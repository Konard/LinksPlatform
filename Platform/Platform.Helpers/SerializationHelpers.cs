using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Platform.Helpers
{
    public static class SerializationHelpers
    {
        static readonly ConcurrentDictionary<Type, XmlSerializer> XmlSerializerCache = new ConcurrentDictionary<Type, XmlSerializer>();

        static XmlSerializer GetOrAddXmlSerializer<T>()
        {
            return XmlSerializerCache.GetOrAdd(typeof(T), type => new XmlSerializer(type));
        }

        public static T DeserializeFromXml<T>(string xmlString)
        {
            using (var reader = new StringReader(xmlString))
                return (T)GetOrAddXmlSerializer<T>().Deserialize(reader);
        }

        public static void SerializeToFile<T>(string path, T obj)
        {
            using (var fileStream = File.Open(path, FileMode.Create))
                GetOrAddXmlSerializer<T>().Serialize(fileStream, obj);
        }

        public static string SerializeAsXmlString<T>(T obj)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
                GetOrAddXmlSerializer<T>().Serialize(writer, obj);
            return sb.ToString();
        }
    }
}
