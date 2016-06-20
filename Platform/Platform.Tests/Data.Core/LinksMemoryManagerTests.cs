using System.IO;
using Platform.Data.Core.Pairs;
using Platform.Helpers;
using Platform.Memory;
using Xunit;

namespace Platform.Tests.Data.Core
{
    public static class LinksMemoryManagerTests
    {
        private static readonly LinksConstants<bool, ulong, long> Constants = Default<LinksConstants<bool, ulong, long>>.Instance;

        [Fact]
        public static void BasicFileMappedMemoryTest()
        {
            var tempFilename = Path.GetTempFileName();

            using (var memoryManager = new UInt64LinksMemoryManager(tempFilename))
                memoryManager.TestBasicMemoryOperations();

            File.Delete(tempFilename);
        }

        [Fact]
        public static void BasicHeapMemoryTest()
        {
            using (var memory = new HeapResizableDirectMemory(UInt64LinksMemoryManager.DefaultLinksSizeStep))
            using (var memoryManager = new UInt64LinksMemoryManager(memory, UInt64LinksMemoryManager.DefaultLinksSizeStep))
                memoryManager.TestBasicMemoryOperations();
        }

        private static void TestBasicMemoryOperations(this ILinksMemoryManager<ulong> memoryManager)
        {
            var link = memoryManager.AllocateLink();
            memoryManager.FreeLink(link);
        }

        [Fact]
        public static void NonexistentReferencesHeapMemoryTest()
        {
            using (var memory = new HeapResizableDirectMemory(UInt64LinksMemoryManager.DefaultLinksSizeStep))
            using (var memoryManager = new UInt64LinksMemoryManager(memory, UInt64LinksMemoryManager.DefaultLinksSizeStep))
                memoryManager.TestNonexistentReferences();
        }

        private static void TestNonexistentReferences(this ILinksMemoryManager<ulong> memoryManager)
        {
            var link = memoryManager.AllocateLink();

            memoryManager.SetLinkValue(link, ulong.MaxValue, ulong.MaxValue);

            var resultLink = Constants.Null;

            memoryManager.Each(foundLink =>
            {
                resultLink = foundLink;
                return Constants.Break;
            }, Constants.Any, ulong.MaxValue, ulong.MaxValue);

            Assert.True(resultLink == link);

            Assert.True(memoryManager.Count(ulong.MaxValue) == 0);

            memoryManager.FreeLink(link);
        }
    }
}
