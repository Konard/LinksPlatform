using System.IO;
using Platform.Data.Core.Pairs;
using Platform.Memory;
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

        [Fact]
        public void HeapMemorySupportTest()
        {
            using (var memory = new HeapResizableDirectMemory(LinksMemoryManager.DefaultLinksSizeStep))
            using (var memoryManager = new LinksMemoryManager(memory, LinksMemoryManager.DefaultLinksSizeStep))
            {
                var link = memoryManager.AllocateLink();
                memoryManager.FreeLink(link);
            }
        }
    }
}
