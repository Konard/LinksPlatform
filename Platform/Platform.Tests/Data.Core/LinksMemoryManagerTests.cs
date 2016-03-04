using System.IO;
using Platform.Data.Core.Pairs;
using Xunit;

namespace Platform.Tests.Data.Core
{
    public class LinksMemoryManagerTests
    {
        [Fact]
        public void BasicMemoryTest()
        {
            var tempFilename = Path.GetTempFileName();

            using (var memoryManager = new LinksMemoryManager(tempFilename))
            {
                var link = memoryManager.AllocateLink();
                memoryManager.FreeLink(link);
            }

            File.Delete(tempFilename);
        }
    }
}
