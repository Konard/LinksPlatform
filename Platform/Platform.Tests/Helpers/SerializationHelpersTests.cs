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

            Assert.True(File.ReadAllText(tempFilename) == "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<anyType xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />");

            File.Delete(tempFilename);
        }

        [Fact]
        public void SerializeAsXmlStringTest()
        {
            var serializedObject = SerializationHelpers.SerializeToXmlString(Default<object>.Instance);
            Assert.True(serializedObject == "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<anyType xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />");
        }
    }
}
