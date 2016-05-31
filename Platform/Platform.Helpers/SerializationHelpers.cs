using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Platform.Helpers
{
    public static class SerializationHelpers
    {
        private static readonly ConcurrentDictionary<Type, XmlSerializer> XmlSerializerCache = new ConcurrentDictionary<Type, XmlSerializer>();

        public static XmlSerializer GetCachedXmlSerializer<T>()
        {
            return XmlSerializerCache.GetOrAdd(typeof(T), type => new XmlSerializer(type));
        }

        public static T DeserializeFromXmlFile<T>(string path)
        {
            using (var reader = File.OpenRead(path))
                return (T)GetCachedXmlSerializer<T>().Deserialize(reader);
        }

        public static T DeserializeFromXmlString<T>(string xmlString)
        {
            using (var reader = new StringReader(xmlString))
                return (T)GetCachedXmlSerializer<T>().Deserialize(reader);
        }

        public static void SerializeToXmlFile<T>(T obj, string path)
        {
            using (var fileStream = File.OpenWrite(path))
                GetCachedXmlSerializer<T>().Serialize(fileStream, obj);
        }

        public static string SerializeToXmlString<T>(T obj)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
                GetCachedXmlSerializer<T>().Serialize(writer, obj);
            return sb.ToString();
        }
    }
}
