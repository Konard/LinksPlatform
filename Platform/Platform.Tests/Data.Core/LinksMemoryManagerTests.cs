using System.IO;
using Platform.Data.Core.Pairs;
using Platform.Memory;
using Xunit;

namespace Platform.Tests.Data.Core
{
    public static class LinksMemoryManagerTests
    {
        [Fact]
        public static void BasicFileMappedMemoryTest()
        {
            var tempFilename = Path.GetTempFileName();

            using (var memoryManager = new LinksMemoryManager(tempFilename))
                memoryManager.TestBasicMemoryOperations();

            File.Delete(tempFilename);
        }

        [Fact]
        public static void BasicHeapMemoryTest()
        {
            using (var memory = new HeapResizableDirectMemory(LinksMemoryManager.DefaultLinksSizeStep))
            using (var memoryManager = new LinksMemoryManager(memory, LinksMemoryManager.DefaultLinksSizeStep))
                memoryManager.TestBasicMemoryOperations();
        }

        private static void TestBasicMemoryOperations(this ILinksMemoryManager<ulong> memoryManager)
        {
            var link = memoryManager.AllocateLink();
            memoryManager.FreeLink(link);
        }
    }
}
