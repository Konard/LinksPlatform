using System;
using System.IO;
using Platform.Helpers;
using Xunit;

namespace Platform.Tests.Helpers
{
    public class SerializationHelpersTests
    {
        [Fact]
        public void SerializeToFileTest()
        {
            var tempFilename = Path.GetTempFileName();

            SerializationHelpers.SerializeToXmlFile(Default<object>.Instance, tempFilename);

#if NET45
            Assert.Equal(File.ReadAllText(tempFilename), $"<?xml version=\"1.0\"?>{Environment.NewLine}<anyType xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" />");
#else
            Assert.Equal(File.ReadAllText(tempFilename), $"<?xml version=\"1.0\" encoding=\"utf-8\"?>{Environment.NewLine}<anyType xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />");
#endif

            File.Delete(tempFilename);
        }

        [Fact]
        public void SerializeAsXmlStringTest()
        {
            var serializedObject = SerializationHelpers.SerializeToXmlString(Default<object>.Instance);

#if NET45
            Assert.Equal(serializedObject, $"<?xml version=\"1.0\" encoding=\"utf-16\"?>{Environment.NewLine}<anyType xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" />");
#else
            Assert.Equal(serializedObject, $"<?xml version=\"1.0\" encoding=\"utf-16\"?>{Environment.NewLine}<anyType xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />");
#endif
        }
    }
}
