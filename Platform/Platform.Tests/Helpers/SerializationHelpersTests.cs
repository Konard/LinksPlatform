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

            SerializationHelpers.SerializeToFile(tempFilename, new object());

            Assert.True(File.ReadAllText(tempFilename) == "<?xml version=\"1.0\"?>\r\n<anyType xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />");

            File.Delete(tempFilename);
        }

        [Fact]
        public void SerializeAsXmlStringTest()
        {
            var serializedObject = SerializationHelpers.SerializeAsXmlString(new object());
            Assert.True(serializedObject == "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<anyType xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />");
        }
    }
}
